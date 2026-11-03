using JwtAuth.Data;
using JwtAuth.Dtos;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<ProfileDto?> GetProfileAsync(Guid userId)
        {
            return await _context.Users
                .Where(user => user.Id == userId)
                .Select(user => new ProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Role = user.Role
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (user is null)
            {
                return null;
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Phone = request.Phone?.Trim();
            user.Email = request.Email.Trim().ToLower();

            await _context.SaveChangesAsync();

            return new ProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role
            };
        }

        public async Task<ProfileDto?> UploadProfilePhotoAsync(
            Guid userId,
            IFormFile file
        )
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (user is null)
            {
                return null;
            }

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads",
                "Profiles"
            );

            Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                DeleteExistingPhoto(user.ProfileImageUrl);
            }

            string extension = Path.GetExtension(file.FileName);

            string fileName =
                $"{userId}-{Guid.NewGuid()}{extension}";

            string filePath = Path.Combine(
                uploadsFolder,
                fileName
            );

            await using var stream = new FileStream(
                filePath,
                FileMode.Create
            );

            await file.CopyToAsync(stream);

            user.ProfileImageUrl =
                $"/uploads/profiles/{fileName}";

            await _context.SaveChangesAsync();

            return MapProfile(user);
        }

        public async Task<bool> DeleteProfilePhotoAsync(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (user is null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                DeleteExistingPhoto(user.ProfileImageUrl);
            }

            user.ProfileImageUrl = null;

            await _context.SaveChangesAsync();

            return true;
        }

        private void DeleteExistingPhoto(string profileImageUrl)
        {
            string relativePath = profileImageUrl
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            string filePath = Path.Combine(
                _environment.ContentRootPath,
                relativePath
            );

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static ProfileDto MapProfile(
            Entities.User user
        )
        {
            return new ProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role
            };
        }


    }
}
using JwtAuth.Data;
using JwtAuth.Dtos.AdminUsers;
using JwtAuth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AdminUserService(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<List<AdminUserDto>> GetAdminUsersAsync()
        {
            return await _context.Users
                .Where(user => user.Role == "Admin")
                .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .Select(user => new AdminUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive
                })
                .ToListAsync();
        }

        public async Task<AdminUserDto> CreateAdminUserAsync(
            CreateAdminUserDto request)
        {
            var email = request.Email
                .Trim()
                .ToLowerInvariant();

            var existingUser = await _context.Users
                .AnyAsync(user => user.Email.ToLower() == email);

            if (existingUser)
            {
                throw new InvalidOperationException(
                    "A user with this email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Role = "Admin"
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return new AdminUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };
        }

        public async Task ResetAdminPasswordAsync(
    Guid adminUserId,
    string newPassword)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Id == adminUserId &&
                    user.Role == "Admin");

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Admin user was not found.");
            }

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    newPassword);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();
        }

        public async Task SetAdminActiveStatusAsync(
    Guid adminUserId,
    bool isActive,
    Guid currentAdminUserId)
        {
            if (adminUserId == currentAdminUserId && !isActive)
            {
                throw new InvalidOperationException(
                    "You cannot deactivate your own admin account.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Id == adminUserId &&
                    user.Role == "Admin");

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Admin user was not found.");
            }

            user.IsActive = isActive;

            if (!isActive)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
            }

            await _context.SaveChangesAsync();
        }
    }
}
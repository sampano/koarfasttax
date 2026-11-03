using JwtAuth.Dtos;

namespace JwtAuth.Services
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetProfileAsync(Guid userId);

        Task<ProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto request);

        Task<ProfileDto?> UploadProfilePhotoAsync(Guid userId,IFormFile file);

        Task<bool> DeleteProfilePhotoAsync(Guid userId);
    }
}

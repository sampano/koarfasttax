using JwtAuth.Dtos.AdminUsers;

namespace JwtAuth.Services
{
    public interface IAdminUserService
    {
        Task<List<AdminUserDto>> GetAdminUsersAsync();

        Task<AdminUserDto> CreateAdminUserAsync(
            CreateAdminUserDto request);

        Task ResetAdminPasswordAsync(
    Guid adminUserId,
    string newPassword);

        Task SetAdminActiveStatusAsync(
    Guid adminUserId,
    bool isActive,
    Guid currentAdminUserId);
    }
}
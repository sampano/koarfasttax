using JwtAuth.Entities;
using JwtAuth.Model;

namespace JwtAuth.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);

        Task<TokenResponseDto?> LoginAsync(UserDto request);

        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);

        Task<string?> CreatePasswordResetTokenAsync(string email);

        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}

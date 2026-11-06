using JwtAuth.Data;
using JwtAuth.Entities;
using JwtAuth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuth.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService) : IAuthService
    {
        private static string HashResetToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes);
        }
        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenRespose(user);
        }

        private async Task<TokenResponseDto> CreateTokenRespose(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)

            };
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return null;
            }

            var user = new User
            {
                Email = request.Email.Trim().ToLower(),
                Role = "Client"
            };

            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.PasswordHash = hashedPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }
        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return null;
            }

            return await CreateTokenRespose(user);
        }
        private async Task<User?> ValidateRefreshTokenAsync(
     Guid userId,
     string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);

            if (user is null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            if (user.RefreshToken != refreshToken
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }
        // Generate a secure random refresh token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        // Generate and save a refresh token for the user
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Set the expiry time for the refresh token
            await context.SaveChangesAsync();
            return refreshToken;
        }
        // Create a JWT token for the authenticated user
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Key")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
               issuer: configuration.GetValue<string>("AppSettings:Issuer"),
               audience: configuration.GetValue<string>("AppSettings:Audience"),
                 claims: claims,
                 expires: DateTime.UtcNow.AddDays(1),
                 signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<string?> CreatePasswordResetTokenAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();

            var user = await context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == normalizedEmail);

            if (user is null)
            {
                return null;
            }

            var randomBytes =
                RandomNumberGenerator.GetBytes(32);

            var resetToken =
                Convert.ToHexString(randomBytes);

            user.PasswordResetToken =
                HashResetToken(resetToken);

            user.PasswordResetTokenExpiry =
                DateTime.UtcNow.AddMinutes(30);

            await context.SaveChangesAsync();

            var client = await context.Clients
            .FirstOrDefaultAsync(c => c.PortalUserId == user.Id);

            if (client is null ||
                string.IsNullOrWhiteSpace(client.GoHighLevelContactId))
            {
                return "GHL_NOT_CONNECTED";
            }

            var resetUrl =
                $"https://portal.koarfasttax.com/reset-password?token={resetToken}";

            await emailService.SendForgotPasswordAsync(
                client.GoHighLevelContactId,
                user.Email,
                resetUrl
            );

            return resetToken;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var hashedToken = HashResetToken(token);

            var user = await context.Users
                .FirstOrDefaultAsync(u =>
                    u.PasswordResetToken == hashedToken
                );

            if (user is null)
            {
                return false;
            }

            if (user.PasswordResetTokenExpiry is null ||
                user.PasswordResetTokenExpiry <= DateTime.UtcNow)
            {
                return false;
            }

            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, newPassword);

            // Make the token unusable after a successful reset.
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            // Also invalidate existing refresh tokens.
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await context.SaveChangesAsync();

            return true;
        }

    }

    
}

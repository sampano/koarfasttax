using JwtAuth.Dtos.AdminUsers;
using JwtAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(
            IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminUserDto>>> GetAdminUsers()
        {
            var admins = await _adminUserService.GetAdminUsersAsync();

            return Ok(admins);
        }

        [HttpPost]
        public async Task<ActionResult<AdminUserDto>>
            CreateAdminUser(CreateAdminUserDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FirstName))
                {
                    return BadRequest(new
                    {
                        message = "First name is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.LastName))
                {
                    return BadRequest(new
                    {
                        message = "Last name is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new
                    {
                        message = "Email is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        message = "Password is required."
                    });
                }

                if (request.Password.Length < 8)
                {
                    return BadRequest(new
                    {
                        message =
                            "Password must be at least 8 characters."
                    });
                }

                var admin =
                    await _adminUserService
                        .CreateAdminUserAsync(request);

                return Ok(admin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id:guid}/password")]
        public async Task<IActionResult> ResetPassword(
    Guid id,
    ResetAdminPasswordDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        message = "Password is required."
                    });
                }

                if (request.Password.Length < 8)
                {
                    return BadRequest(new
                    {
                        message = "Password must be at least 8 characters."
                    });
                }

                await _adminUserService.ResetAdminPasswordAsync(
                    id,
                    request.Password);

                return Ok(new
                {
                    message = "Admin password reset successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
    Guid id,
    UpdateAdminStatusDto request)
        {
            try
            {
                var currentUserIdValue =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(currentUserIdValue, out var currentUserId))
                {
                    return Unauthorized();
                }

                await _adminUserService.SetAdminActiveStatusAsync(
                    id,
                    request.IsActive,
                    currentUserId);

                return Ok(new
                {
                    message = request.IsActive
                        ? "Admin account activated successfully."
                        : "Admin account deactivated successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
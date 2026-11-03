using JwtAuth.Dtos;
using JwtAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            Guid userId = GetCurrentUserId();

            var profile = await _profileService.GetProfileAsync(userId);

            if (profile is null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

        [HttpPut]
        public async Task<ActionResult<ProfileDto>> UpdateProfile(
            UpdateProfileDto request
        )
        {
            Guid userId = GetCurrentUserId();

            var profile = await _profileService.UpdateProfileAsync(
                userId,
                request
            );

            if (profile is null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

        [HttpPost("photo")]
        public async Task<ActionResult<ProfileDto>> UploadProfilePhoto(
            IFormFile file
        )
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest("Please select an image.");
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest(
                    "Profile image must be 5 MB or smaller."
                );
            }

            string[] allowedContentTypes =
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(file.ContentType))
            {
                return BadRequest(
                    "Only JPG, PNG, and WEBP images are allowed."
                );
            }

            Guid userId = GetCurrentUserId();

            var profile =
                await _profileService.UploadProfilePhotoAsync(
                    userId,
                    file
                );

            if (profile is null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

        [HttpDelete("photo")]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            Guid userId = GetCurrentUserId();

            bool deleted =
                await _profileService.DeleteProfilePhotoAsync(
                    userId
                );

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            return Guid.Parse(userId!);
        }
    }
}
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Commands.UpdateProfile;
using UteLearningHub.Application.Features.Account.Queries.GetProfile;

namespace UteLearningHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;

        public AccountController(IMediator mediator, IWebHostEnvironment environment)
        {
            _mediator = mediator;
            _environment = environment;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            var query = new GetProfileQuery { UserId = null };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<ProfileDto>> GetProfileById(Guid userId)
        {
            var query = new GetProfileQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("avatar")]
        [Authorize]
        public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type. Only image files are allowed.");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size must be less than 5MB");
            }

            try
            {
                var uploadDir = Path.Combine(_environment.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadDir, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var avatarUrl = $"/images/avatars/{fileName}";
                return Ok(new { url = avatarUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading avatar: {ex.Message}");
            }
        }
    }
}

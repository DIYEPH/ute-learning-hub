using System.IO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Commands.UpdateProfile;
using UteLearningHub.Application.Features.Account.Queries.GetProfile;
using UteLearningHub.Application.Services.FileStorage;

namespace UteLearningHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorageService;

        public AccountController(IMediator mediator, IFileStorageService fileStorageService)
        {
            _mediator = mediator;
            _fileStorageService = fileStorageService;
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

        // Upload avatar endpoint removed: use /api/File + UpdateProfile instead
    }
}

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Features.Account.Queries.GetProfile;

namespace UteLearningHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator) { 
            _mediator = mediator;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<GetProfileResponse>> GetProfile()
        {
            var query = new GetProfileQuery { UserId = null };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<GetProfileResponse>> GetProfileById(Guid userId)
        {
            var query = new GetProfileQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}

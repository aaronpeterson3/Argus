using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace Argus.Features.Tenants.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTenantCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(result.Value);
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenant(string id)
        {
            var query = new GetTenantQuery(id);
            var result = await _mediator.Send(query);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(result.Value);
        }

        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateTenantProfileCommand command)
        {
            command = command with { TenantId = id };
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok();
        }
    }
}
using MediatR;
using Mews.Job.Scheduler.Features.Authentication;
using Mews.Job.Scheduler.Swagger.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.Features.Internal;

[ApiController]
[Route("api/system")]
[RequiresAuthenticationHeader]
[ServiceFilter(typeof(AccessTokenAuthorizationFilter))]
public sealed class SystemController : ControllerBase
{
    private readonly IMediator _mediator;

    public SystemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("info")]
    [ProducesResponseType(200)]
    public async Task<IResult> GetSystemInfo()
    {
        var result = await _mediator.Send(new GetSystemInfoQuery());
        return result;
    }
}

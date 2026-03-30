using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.Features.Internal;

[ApiController]
[Authorize]
[Route("api/s2s-test")]
public class S2STestController : ControllerBase
{
    [HttpGet("get-test")]
    [ProducesResponseType(200)]
    public IResult GetTest()
    {
        return Results.Ok("Test result");
    }
}

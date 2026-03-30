using System.Diagnostics;
using MediatR;
using Mews.Job.Scheduler.Core.Application.JobProcessing.Commands;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Features.Authentication;
using Mews.Job.Scheduler.Features.Jobs.Dtos;
using Mews.Job.Scheduler.Observability;
using Mews.Job.Scheduler.Swagger.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.Features.Jobs;

[ApiController]
[Route("api/processing")]
[RequiresAuthenticationHeader]
[ServiceFilter(typeof(AccessTokenAuthorizationFilter))]
public sealed class ProcessingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProcessingController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ConfirmProcessingResultDto), StatusCodes.Status200OK)]
    [Route("confirmProcessing")]
    public async Task<ActionResult<ConfirmProcessingResultDto>> ConfirmProcessingAsync(ConfirmProcessingParametersDto parameters, CancellationToken cancellationToken)
    {
        using var activity = JobProcessingDiagnostics.Source.StartActivity(ActivityKind.Internal, parentContext: Activity.Current?.Context ?? default);
        
        var command = new ConfirmProcessingCommand
        {
            JobId = parameters.JobId,
            ExecutionTransactionIdentifier = parameters.TransactionIdentifier
        };
        var result = await _mediator.Send(command, cancellationToken);

        return new ConfirmProcessingResultDto
        {
            JobExecutionId = result.Id,
            ExecutionStartUtc = result.StartUtc
        };
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("confirmResult")]
    public async Task<ActionResult> ConfirmResultAsync(ConfirmResultParametersDto resultParameters, CancellationToken cancellationToken)
    {
        using var activity = JobProcessingDiagnostics.Source.StartActivity(ActivityKind.Internal, parentContext: Activity.Current?.Context ?? default);

        var command = new ConfirmResultCommand
        {
            JobExecutionId = resultParameters.JobExecutionId,
            State = ApiDtoMapper.Convert<JobExecutionStateDto, JobExecutionState>(resultParameters.Parameters.State),
            Tag = resultParameters.Parameters.Tag,
            DeleteJob = resultParameters.Parameters.DeleteJob,
            FutureRunData = resultParameters.Parameters.FutureRunData
        };
        
        await _mediator.Send(command, cancellationToken);
        
        return Ok();
    }
}

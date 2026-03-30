using System.Linq.Expressions;
using MediatR;
using Mews.Job.Scheduler.Core.Application.JobExecutions.Commands;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Features.Authentication;
using Mews.Job.Scheduler.Features.Jobs.Dtos;
using Mews.Job.Scheduler.Swagger.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.Features.Jobs;

[ApiController]
[Route("api/jobExecutions")]
[RequiresAuthenticationHeader]
[ServiceFilter(typeof(AccessTokenAuthorizationFilter))]
public sealed class JobExecutionController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public JobExecutionController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobExecutionGetResultDto>> GetAsync(JobExecutionGetParametersDto parameters, CancellationToken cancellationToken)
    {
        if (parameters.StartInterval?.StartUtc is not null &&
            parameters.StartInterval?.EndUtc is not null &&
            parameters.StartInterval.StartUtc >= parameters.StartInterval.EndUtc)
        {
            throw new BadHttpRequestException($"Invalid time interval, {nameof(parameters.StartInterval.StartUtc)} cannot be after {nameof(parameters.StartInterval.EndUtc)}");
        }
        
        var command = ToGetFilteredCommand(parameters);
        var executions = await _mediator.Send(command, cancellationToken);

        return new JobExecutionGetResultDto
        {
            JobExecutions = executions.Select(e => new JobExecutionDto
            {
                Id = e.Id,
                Job = ApiDtoMapper.ToJobDto(e.Job),
                State = ApiDtoMapper.Convert<JobExecutionState, JobExecutionStateDto>(e.State),
                StartUtc = e.StartUtc,
                EndUtc = e.EndUtc,
                TransactionIdentifier = e.TransactionIdentifier,
                Tag = e.Tag,
                ExecutorTypeNameValue = e.ExecutorTypeNameValue,
                CreatedUtc = e.CreatedUtc,
                UpdatedUtc = e.UpdatedUtc,
                DeletedUtc = e.DeletedUtc,
                CreatorProfileId = e.CreatorProfileId,
                UpdaterProfileId = e.UpdaterProfileId,
                IsDeleted = e.IsDeleted
            })
        };
    }

    private static JobExecutionGetFilteredCommand ToGetFilteredCommand(JobExecutionGetParametersDto parametersDto)
    {
        return new JobExecutionGetFilteredCommand
        {
            Filters = new JobExecutionFilters
            {
                Ids = parametersDto.Ids,
                JobIds = parametersDto.JobIds,
                States = ApiDtoMapper.Convert<JobExecutionStatesDto, JobExecutionStates>(parametersDto.States),
                StartInterval = parametersDto.StartInterval != null ? new DateTimeInterval(parametersDto.StartInterval.StartUtc, parametersDto.StartInterval.EndUtc) : null,
                ExecutorTypeNames = parametersDto.ExecutorTypeNames,
                ShowDeleted = parametersDto.ShowDeleted,
                Limitation = new Limitation<JobExecution>
                {
                    Count = parametersDto.Limitation.Count,
                    StartIndex = parametersDto.Limitation.StartIndex,
                    EagerLoad = new EagerLoad<JobExecution>
                    {
                        Selectors = new List<Expression<Func<JobExecution, object>>> { jobExecution => jobExecution.Job }
                    }
                }
            }
        };
    }
}

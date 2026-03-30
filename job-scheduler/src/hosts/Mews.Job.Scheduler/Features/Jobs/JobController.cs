using MediatR;
using Mews.Job.Scheduler.Core.Application.Jobs.Commands;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Features.Authentication;
using Mews.Job.Scheduler.Features.Jobs.Dtos;
using Mews.Job.Scheduler.Swagger.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.Features.Jobs;

[ApiController]
[Route("api/jobs")]
[RequiresAuthenticationHeader]
[ServiceFilter(typeof(AccessTokenAuthorizationFilter))]
public sealed class JobController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var command = new JobGetCommand { Id = id };
        var job = await _mediator.Send(command, cancellationToken);

        return ApiDtoMapper.ToJobDto(job);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobGetResultDto>> Get(JobGetParametersDto parameters, CancellationToken cancellationToken)
    {
        var command = ToGetFilteredCommand(parameters);
        var jobs = await _mediator.Send(command, cancellationToken);

        return new JobGetResultDto
        {
            Jobs = jobs.Select(ApiDtoMapper.ToJobDto)
        };
    }

    [HttpPost("filter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobGetResultDto>> GetFiltered([FromBody] JobGetParametersDto parameters, CancellationToken cancellationToken)
    {
        var command = ToGetFilteredCommand(parameters);
        var jobs = await _mediator.Send(command, cancellationToken);

        return new JobGetResultDto
        {
            Jobs = jobs.Select(ApiDtoMapper.ToJobDto)
        };
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobCreateResultDto>> Create([FromBody] JobCreateParametersDto parameters, CancellationToken cancellationToken)
    {
        var command = ToCreateCommand(parameters);
        var jobs = await _mediator.Send(command, cancellationToken);

        return new JobCreateResultDto
        {
            CreatedJobs = jobs.Select(ApiDtoMapper.ToJobDto)
        };
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobUpdateResultDto>> Update(Guid id, [FromBody] JobUpdateParametersDto parameters, CancellationToken cancellationToken)
    {
        var updates = new Dictionary<Guid, JobUpdateDataDto> { { id, parameters.UpdatedJob } };
        var command = ToUpdateCommand(updates);
        var jobs = await _mediator.Send(command, cancellationToken);

        return new JobUpdateResultDto
        {
            UpdatedJobs = jobs.Select(ApiDtoMapper.ToJobDto)
        };
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, [FromBody] JobDeleteParametersDto parameters, CancellationToken cancellationToken)
    {
        var command = new JobDeleteCommand
        {
            Ids = new [] { id },
            UpdaterProfileId = parameters.UpdaterProfileId
        };
        await _mediator.Send(command, cancellationToken);

        return new NoContentResult();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromBody] JobDeleteBatchParametersDto parameters, CancellationToken cancellationToken)
    {
        var command = new JobDeleteCommand
        {
            Ids = parameters.JobIds.ToList(),
            UpdaterProfileId = parameters.UpdaterProfileId
        };
        await _mediator.Send(command, cancellationToken);

        return new NoContentResult();
    }

    private static JobGetFilteredCommand ToGetFilteredCommand(JobGetParametersDto parametersDto)
    {
        if (parametersDto.StartUtc is not null &&
            parametersDto.EndUtc is not null &&
            parametersDto.StartUtc >= parametersDto.EndUtc)
        {
            throw new BadHttpRequestException($"Invalid time interval, {nameof(parametersDto.StartUtc)} cannot be after {nameof(parametersDto.EndUtc)}");
        }

        var limitation = parametersDto.Limitation is not null
            ? new Limitation
            {
                Count = parametersDto.Limitation.Count,
                StartIndex = parametersDto.Limitation.StartIndex
            }
            : null;

        var filters = new JobFilters(
            ids : parametersDto.Ids,
            name: parametersDto.Name,
            executorTypeNames: parametersDto.ExecutorTypeNames,
            states: ApiDtoMapper.Convert<JobStatesDto, JobStates>(parametersDto.States),
            startUtc: parametersDto.StartUtc,
            endUtc: parametersDto.EndUtc,
            showDeleted: parametersDto.ShowDeleted,
            limitation: limitation
        );

        return new JobGetFilteredCommand
        {
            Filters = filters
        };
    }

    private static JobCreateCommand ToCreateCommand(JobCreateParametersDto parametersDto)
    {
        var createParameters = parametersDto.Jobs.Select(d => new JobCreateParameters(
            startUtc: d.StartUtc,
            executorTypeName: d.ExecutorTypeName,
            team: d.Team,
            maxExecutionTime: d.MaxExecutionTime,
            name: d.Name,
            period: d.Period,
            options: ApiDtoMapper.Convert<JobOptionsDto, JobOptions>(d.Options),
            data: d.Data,
            creatorProfileId: d.CreatorProfileId
        ));

        return new JobCreateCommand
        {
            CreateParameters = createParameters.ToList()
        };
    }

    private static JobUpdateCommand ToUpdateCommand(IDictionary<Guid, JobUpdateDataDto> updatesByIds)
    {
        var updates = new Dictionary<Guid, JobUpdateParameters>(updatesByIds.Count);
        foreach (var (id, dto) in updatesByIds)
        {
            var update = new JobUpdateParameters(
                Name: dto.Name,
                ExecutorTypeName: dto.ExecutorTypeName,
                StartUtc: dto.StartUtc,
                Period: dto.Period,
                MaxExecutionTime: dto.MaxExecutionTime,
                Options: ApiDtoMapper.Convert<JobOptionsDto, JobOptions>(dto.Options),
                Data: dto.Data,
                UpdaterProfileId: dto.UpdaterProfileId
            );
            updates.Add(id, update);
        }

        return new JobUpdateCommand
        {
            JobUpdates = updates
        };
    }
}

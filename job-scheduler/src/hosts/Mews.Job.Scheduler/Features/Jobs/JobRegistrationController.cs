using System.Collections.Immutable;
using MediatR;
using Mews.Job.Scheduler.Core.Application.Registration;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Features.Authentication;
using Mews.Job.Scheduler.Swagger.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.Features.Jobs;

[ApiController]
[Route("api/registration")]
[RequiresAuthenticationHeader]
[ServiceFilter(typeof(AccessTokenAuthorizationFilter))]
public sealed class JobRegistrationController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobRegistrationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dtos.JobRegistrationResultDto>> RegisterAsync([FromBody] Dtos.JobRegistrationParametersDto parameters, CancellationToken cancellationToken)
    {
        var command = ToRegistrationCommand(parameters);
        var result = await _mediator.Send(command, cancellationToken);

        return new Dtos.JobRegistrationResultDto
        {
            CreatedJobs = result.CreatedJobs.Select(ApiDtoMapper.ToJobDto),
            DeletedJobs = result.DeletedJobs.Select(ApiDtoMapper.ToJobDto)
        };
    }

    private static JobRegistrationCommand ToRegistrationCommand(Dtos.JobRegistrationParametersDto parametersDto)
    {
        var recognizedExecutors = parametersDto.RecognizedJobExecutorsMetadata.Select(dto => new ExecutorCreateParameters(dto.Type, dto.Team));
        var jobs = parametersDto.JobsToRegister.Select(dto => new JobCreateParameters(
            startUtc: dto.StartUtc,
            executorTypeName: dto.ExecutorTypeName,
            team: dto.Team,
            maxExecutionTime: dto.MaxExecutionTime,
            name: dto.Name,
            period: dto.Period,
            options: ApiDtoMapper.Convert<Dtos.JobOptionsDto, JobOptions>(dto.Options),
            data: dto.Data,
            creatorProfileId: parametersDto.UpdaterProfileId
        ));

        return new JobRegistrationCommand
        {
            RecognizedExecutorsMetadata = recognizedExecutors.ToList(),
            JobsToRegister = jobs.ToList(),
            UpdaterProfileId = parametersDto.UpdaterProfileId
        };
    }
}

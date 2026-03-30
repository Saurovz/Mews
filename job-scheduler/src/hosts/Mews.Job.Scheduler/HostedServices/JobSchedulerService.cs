using Mews.Atlas.Alerting;
using Mews.Job.Scheduler.Domain.JobScheduler;
using Mews.Job.Scheduler.Observability;

namespace Mews.Job.Scheduler.HostedServices;

public sealed class JobSchedulerService : TimedHostedService
{
    private readonly IServiceScopeFactory _serviceScopes;

    public JobSchedulerService(
        ILogger<JobSchedulerService> logger,
        IIncidentReporter incidentReporter,
        [FromKeyedServices(nameof(JobSchedulerService))] TimedHostedServiceConfiguration configuration,
        IServiceScopeFactory serviceScopes)
        : base(logger, incidentReporter, configuration)
    {
        _serviceScopes = serviceScopes;
    }

    public override async Task ExecuteWork(CancellationToken applicationStoppingToken)
    {
        using var activity = JobSchedulerDiagnostics.Source.StartActivity($"{nameof(JobSchedulerService)}");
        using var scope = _serviceScopes.CreateScope();
        var jobScheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();

        await jobScheduler.ScheduleNextBatchAsync(applicationStoppingToken);
    }
}

using Mews.Atlas.Temporal;
using Microsoft.Extensions.Options;
using Temporalio.Extensions.Hosting;

namespace Mews.Job.Scheduler.Workflows.JobTimeoutHandler;

public static class DependencyInjectionExtensions
{
    public static ITemporalWorkerServiceOptionsBuilder AddJobTimeoutHandler(this ITemporalWorkerServiceOptionsBuilder workerBuilder, IConfiguration configuration)
    {
        workerBuilder.Services.AddOptions<JobTimeoutHandlerWorkflowConfiguration>()
            .Bind(configuration.GetSection("JobTimeoutHandlerWorkflow"))
            .Validate(c => c.Period > TimeSpan.Zero);

        workerBuilder
            .AddWorkflow<JobTimeoutHandlerWorkflow>()
            .AddScopedActivities<JobTimeoutHandlerActivity>()
            .AddSchedule(provider => JobTimeoutHandlerSchedule.CreateDefinition(
                provider.GetRequiredService<IOptions<JobTimeoutHandlerWorkflowConfiguration>>().Value.Period));
        return workerBuilder;
    }
}

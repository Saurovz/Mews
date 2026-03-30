using Mews.Job.Scheduler.Infrastructure;
using Pulumi;

await Deployment.RunAsync(Infrastructure.Create!);

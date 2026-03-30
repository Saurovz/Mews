using Mews.Job.Scheduler.BuildingBlocks.Infrastructure;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Mews.Job.Scheduler.MigrationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddDbContextPool<JobSchedulerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("JobSchedulerDatabase"), sqlOptions => {
    // Workaround for https://github.com/dotnet/aspire/issues/1023
    sqlOptions.ExecutionStrategy(c => new RetryingSqlServerRetryingExecutionStrategy(c));
}));
builder.EnrichSqlServerDbContext<JobSchedulerDbContext>(settings =>
    // Disable Aspire default retries as we're using a custom execution strategy
    settings.DisableRetry = true
);

var host = builder.Build();
host.Run();

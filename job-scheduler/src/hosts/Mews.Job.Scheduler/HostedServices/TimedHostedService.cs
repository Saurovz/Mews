using System.Diagnostics;
using Mews.Atlas.Alerting;
using Mews.Job.Scheduler.BuildingBlocks.Domain.PlatformTeams;

namespace Mews.Job.Scheduler.HostedServices;

[Obsolete("Temporal.io should be used instead.")]
public abstract class TimedHostedService : BackgroundService
{
    private readonly ILogger<TimedHostedService> _logger;
    private readonly IIncidentReporter _incidentReporter;

    protected TimedHostedService(
        ILogger<TimedHostedService> logger,
        IIncidentReporter incidentReporter,
        TimedHostedServiceConfiguration configuration)
    {
        _logger = logger;
        _incidentReporter = incidentReporter;

        Timer = new PeriodicTimer(configuration.Period);
        StoppingBehavior = configuration.StoppingBehavior;
        ApplicationStoppingTimeout = TimeSpan.FromSeconds(10);
        IsEnabled = configuration.IsEnabled;
    }

    private PeriodicTimer? Timer { get; }
    private WorkerStoppingBehavior StoppingBehavior { get; }
    private TimeSpan ApplicationStoppingTimeout { get; }
    private bool IsEnabled { get; }

    public abstract Task ExecuteWork(CancellationToken applicationStoppingToken);

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        if (IsEnabled)
        {
            _logger.LogDebug("Starting.");

            await base.StartAsync(stoppingToken);

            _logger.LogDebug("Started.");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        if (IsEnabled)
        {
            _logger.LogDebug("Stopping.");

            Timer?.Dispose();
            await DelayAsync();

            await base.StopAsync(stoppingToken);

            _logger.LogDebug("Stopped.");
        }
    }

    public override void Dispose()
    {
        _logger.LogDebug("Disposing.");

        Timer?.Dispose();
        base.Dispose();

        _logger.LogDebug("Disposed.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await Timer!.WaitForNextTickAsync(stoppingToken))
            {
                await TryExecuteAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Application stopping.");
        }
    }

    private async Task TryExecuteAsync(CancellationToken applicationStoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug($"{nameof(ExecuteWork)} started.");

            await ExecuteWork(applicationStoppingToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            _incidentReporter.UnhandledException(e, team: PlatformTeams.Tooling);
        }
        finally
        {
            _logger.LogDebug("{MethodName} finished. Execution took {ElapsedMs} milliseconds.", nameof(ExecuteWork), stopwatch.ElapsedMilliseconds);
        }
    }

    private Task DelayAsync()
    {
        return StoppingBehavior switch
        {
            WorkerStoppingBehavior.Immediate => Task.CompletedTask,
            WorkerStoppingBehavior.WaitForShutdown => Task.Delay(ApplicationStoppingTimeout),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

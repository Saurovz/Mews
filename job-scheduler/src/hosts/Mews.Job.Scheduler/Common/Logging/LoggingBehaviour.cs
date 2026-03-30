using MediatR.Pipeline;

namespace Mews.Job.Scheduler.Common.Logging;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;

    public LoggingBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("MediatR request: {Name} {@Request}", requestName, request);
        return Task.CompletedTask;
    }
}

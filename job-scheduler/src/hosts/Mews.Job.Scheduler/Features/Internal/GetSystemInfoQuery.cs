using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using MediatR;

namespace Mews.Job.Scheduler.Features.Internal;

public sealed record GetSystemInfoQuery : IRequest<IResult>;

internal sealed class GetSystemInfoQueryHandler : IRequestHandler<GetSystemInfoQuery, IResult>
{
    public Task<IResult> Handle(GetSystemInfoQuery request, CancellationToken cancellationToken)
    {
        var result = new SystemInfoDto
        {
            AspNetVersion = RuntimeEnvironment.GetSystemVersion(),
            FullTrust = AppDomain.CurrentDomain.IsFullyTrusted,
            ServerTimeZone = TimeZoneInfo.Local.StandardName,
            LocalServerTime = DateTime.Now,
            NonpagedSystemMemorySize64 = Process.GetCurrentProcess().NonpagedSystemMemorySize64,
            MemoryUsagePrivate = Process.GetCurrentProcess().PrivateMemorySize64,
            MemoryUsageWorkingSet = Process.GetCurrentProcess().WorkingSet64,
            GarbageCollectorMode = GCSettings.IsServerGC == true ? "server" : "workstation",
            OperationSystem = Environment.OSVersion.VersionString,
            Is64BitProcess = Environment.Is64BitProcess,
            Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            UserDomainName = Environment.UserDomainName,
            UtcTime = DateTime.UtcNow,
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        };

        return Task.FromResult(Results.Ok(result));
    }
}

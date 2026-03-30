using System.Runtime.Serialization;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

[Flags]
public enum JobOptionsDto
{
    [EnumMember(Value = "None")]
    None = 0,
    
    [EnumMember(Value = "ParallelExecution")]
    ParallelExecution = 1 << 0,
    
    [EnumMember(Value = "TimeoutRetryDisabled")]
    TimeoutRetryDisabled = 1 << 1,
    
    [EnumMember(Value = "IsFatal")]
    IsFatal = 1 << 2,
    
    [EnumMember(Value = "TimeoutAsWarning")]
    TimeoutAsWarning = 1 << 3
}

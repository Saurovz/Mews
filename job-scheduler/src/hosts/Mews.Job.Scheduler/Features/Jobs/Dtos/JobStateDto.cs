using System.Runtime.Serialization;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public enum JobStateDto
{
    Pending = 0,
    InProgress = 1,
    Executed = 2,
    Scheduled = 3
}

[Flags]
public enum JobStatesDto
{
    [EnumMember(Value = "Pending")]
    Pending = 1 << 0,
    
    [EnumMember(Value = "InProgress")]
    InProgress = 1 << 1,
    
    [EnumMember(Value = "Executed")]
    Executed = 1 << 2,
    
    [EnumMember(Value = "Scheduled")]
    Scheduled = 1 << 3
}

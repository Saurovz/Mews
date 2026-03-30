using System.Runtime.Serialization;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public enum JobExecutionStateDto
{
    InProgress = 0,
    Success = 1,
    Warning = 2,
    Error = 3,
    Timeout = 4
}

[Flags]
public enum JobExecutionStatesDto
{
    [EnumMember(Value = "InProgress")]
    InProgress = 1 << 0,

    [EnumMember(Value = "Success")]
    Success = 1 << 1,

    [EnumMember(Value = "Warning")]
    Warning = 1 << 2,

    [EnumMember(Value = "Error")]
    Error = 1 << 3,

    [EnumMember(Value = "Timeout")]
    Timeout = 1 << 4
}

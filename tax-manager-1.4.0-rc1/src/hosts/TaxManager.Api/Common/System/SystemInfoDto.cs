namespace TaxManager.Common.System;

public class SystemInfoDto
{
    public string? Version { get; internal set; }

    public string? OperationSystem { get; internal set; }
    
    public string? AspNetVersion { get; internal set; }
    
    public bool FullTrust { get; internal set; }
    
    public string? ServerTimeZone { get; internal set; }
    
    public DateTime LocalServerTime { get; internal set; }
    
    public DateTime UtcTime { get; internal set; }
    
    public long MemoryUsagePrivate { get; internal set; }
    
    public long MemoryUsageWorkingSet { get; internal set; }

    public string? GarbageCollectorMode { get; internal set; }
    
    public bool Is64BitProcess { get; internal set; }
    
    public string? MachineName { get; internal set; }
    
    public bool Is64BitOperatingSystem { get; internal set; }
    
    public int ProcessorCount { get; internal set; }
    
    public string? UserDomainName { get; internal set; }
    
    public long NonpagedSystemMemorySize64 { get; internal set; }
}

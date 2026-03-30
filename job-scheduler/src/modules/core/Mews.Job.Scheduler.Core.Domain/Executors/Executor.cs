using Mews.Job.Scheduler.BuildingBlocks.Domain;
using Mews.Job.Scheduler.BuildingBlocks.Domain.Guids;

namespace Mews.Job.Scheduler.Domain.Executors;

public sealed class Executor : Entity<Guid>
{
    public string Type { get; set; }

    public string Team { get; set; }

    public DateTime? DeletedUtc { get; set; }
    
    public ICollection<Jobs.Job> Jobs { get; set; }
    
    public static Executor Create(string type, string team)
    {
        return new Executor
        {
            Id = SequentialGuid.Create(),
            Type = type,
            Team = team
        };
    }
    
    public void Delete(DateTime nowUtc)
    {
        DeletedUtc = nowUtc;
    }

    public void Restore()
    {
        DeletedUtc = null;
    }
    
    public void UpdateTeam(string team)
    {
        Team = team;
    }
}

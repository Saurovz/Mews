using Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;
using Mews.Job.Scheduler.Domain.Executors;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore;

/*
 * How to generate migrations:
 *
 *   1. Start the Aspire Host.
 *   2. You can use the 'dotnet ef' command directly from the root of the repository
 *   3. List pending migrations - dotnet ef migrations list -p  .\src\modules\core\Mews.Job.Scheduler.Core.EntityFrameworkCore\Mews.Job.Scheduler.Core.EntityFrameworkCore.csproj
 *   4. Create migrations - dotnet ef migrations add '{{NameOfTheMigration}}' -p  .\src\modules\core\Mews.Job.Scheduler.Core.EntityFrameworkCore\Mews.Job.Scheduler.Core.EntityFrameworkCore.csproj
 *   5. Remove the last migration (if is not already applied) - dotnet ef migrations remove -p  .\src\modules\core\Mews.Job.Scheduler.Core.EntityFrameworkCore\Mews.Job.Scheduler.Core.EntityFrameworkCore.csproj
 *   6. Apply migrations - dotnet ef database update -p  .\src\modules\core\Mews.Job.Scheduler.Core.EntityFrameworkCore\Mews.Job.Scheduler.Core.EntityFrameworkCore.csproj
 *   7. Drop database - reset local data (optional) - dotnet ef database drop -p  .\src\modules\core\Mews.Job.Scheduler.Core.EntityFrameworkCore\Mews.Job.Scheduler.Core.EntityFrameworkCore.csproj
 */

public sealed class JobSchedulerDbContext : DbContext
{
    public JobSchedulerDbContext(DbContextOptions<JobSchedulerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain.Jobs.Job> Jobs { get; set; } = default!;
    public DbSet<Domain.JobExecutions.JobExecution> JobExecutions { get; set; } = default!;

    public DbSet<Domain.Executors.Executor> Executors { get; set; } = default!;

    public new async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SqlHelpers.CheckedAction(async ct => await base.SaveChangesAsync(ct), cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelJobs(modelBuilder);
        ModelJobExecutions(modelBuilder);
        ModelExecutors(modelBuilder);
    }

    private void ModelJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Jobs.Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Job");

            entity.HasIndex(e => e.CreatedUtc);
            entity.HasIndex(e => e.CreatorProfileId);
            entity.HasIndex(e => new { e.NameNew, e.State });
            entity.HasIndex(e => new { e.StartUtc, e.State, e.NameNew }).HasFilter("([IsDeleted]=(0))"); // Composite index used for cleaner, UI.
            entity.HasIndex(e => new { e.State, e.IsDeleted }); // Used for JobTimeoutHandler service.
            entity.HasIndex(e => e.UpdaterProfileId);
            entity.HasIndex(e => new { e.Id, e.EntityVersion });

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EntityVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate().HasDefaultValue(new byte[] { 0 });
            entity.Property(e => e.StartUtc).IsRequired();
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.Options).IsRequired();
            entity.Ignore(e => e.MaxExecutionTime);
            entity.Property(e => e.MaxExecutionTimeValue).HasMaxLength(36).IsRequired();
            entity.Property(e => e.NameNew).HasMaxLength(255);
            entity.Ignore(e => e.Period);
            entity.Property(e => e.PeriodValue).HasMaxLength(36);
            
            entity.HasOne(j => j.Executor)
                .WithMany(e => e.Jobs)
                .HasForeignKey(j => j.ExecutorId);
        });
    }

    private void ModelJobExecutions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.JobExecutions.JobExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("JobExecution");

            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => new { e.ExecutorTypeNameValue, e.State, e.StartUtc })
                .IncludeProperties(e => new { e.JobId, e.EndUtc, e.TransactionIdentifier, e.CreatedUtc, e.UpdatedUtc, e.DeletedUtc, e.CreatorProfileId, e.UpdaterProfileId, e.IsDeleted }); // Used for experimental UI query.
            
            entity.HasIndex(e => new { e.JobId, e.State, e.IsDeleted }).HasFilter("([IsDeleted]=(0))"); // Used for JobTimeoutHandler service.
            entity.HasIndex(e => new { e.JobId, e.StartUtc });
            entity.HasIndex(e => e.JobId).HasFilter("([State]=(0))");
            entity.HasIndex(e => new { e.JobId, e.State }).HasFilter("([State]=(0))");
            entity.HasIndex(e => e.StartUtc);
            entity.HasIndex(e => new { e.StartUtc, e.ExecutorTypeNameValue, e.State });
            entity.HasIndex(e => new { e.StartUtc, e.JobId });
            entity.HasIndex(e => new { e.StartUtc, e.State });
            entity.HasIndex(e => new { e.JobId, e.TransactionIdentifier }); // Used for JobProcessing's ConfirmProcessing.
            entity.HasIndex(e => e.CreatorProfileId);
            entity.HasIndex(e => e.UpdaterProfileId);

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.StartUtc).IsRequired();
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.ExecutorTypeNameValue).HasMaxLength(64).IsRequired();
            entity.Property(e => e.TransactionIdentifier).HasMaxLength(64);

            entity.HasOne(d => d.Job)
                .WithMany(p => p.JobExecutions)
                .HasForeignKey(d => d.JobId);
        });
    }
    
    private void ModelExecutors(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Executor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Executor");
            
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Type).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Team).HasMaxLength(64).IsRequired();
            entity.Property(e => e.DeletedUtc);
        });
    }
}

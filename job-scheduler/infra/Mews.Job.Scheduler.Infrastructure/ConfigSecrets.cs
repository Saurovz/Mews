namespace Mews.Job.Scheduler.Infrastructure;

public sealed record ConfigSecrets(string SqlServerAdminPassword, string ContainerRegistryPassword);
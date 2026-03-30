namespace TaxManager.Infrastructure;

public sealed record ConfigSecrets(string SqlServerAdminPassword, string ContainerRegistryPassword);
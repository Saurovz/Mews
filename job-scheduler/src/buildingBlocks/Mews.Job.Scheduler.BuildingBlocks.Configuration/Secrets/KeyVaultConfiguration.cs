namespace Mews.Job.Scheduler.Configuration.Secrets;

public sealed class KeyVaultConfiguration
{
    public const string ValueFieldName = "KEY_VAULT_URI_MAIN";
    public const string SectionName = "KeyVault";

    public string? Uri { get; set; }
}

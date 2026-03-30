namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Authentication;

public sealed class AuthenticationTokensConfiguration
{
    public const string SectionName = "AuthenticationTokensConfiguration";

    public IEnumerable<string>? AccessTokens { get; set; }
}

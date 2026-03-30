namespace Mews.Job.Scheduler.BuildingBlocks.Domain.PlatformTeams;

public static class PlatformTeams
{
    public const string Tooling = nameof(PlatformTeam.Tooling);
}

public enum PlatformTeam
{
    Accounts,
    Analytics,
    BookingEngine,
    Connectivity,
    DataPlatform,
    DataProjects,
    DesignSystem,
    Distribution,
    EnterpriseIntegrations,
    Expansion,
    Finance,
    FinancialServices,
    GuestPortal,
    Infrastructure,
    Internal,
    Kiosk,
    PaymentExperiences,
    PaymentProcessing,
    Payouts,
    PortfolioManagement,
    Productivity,
    Quality,
    Reservations,
    Security,
    Services,
    Spacetime,
    Tooling,
    Users
}

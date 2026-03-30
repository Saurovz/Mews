using Mews.Job.Scheduler.Aspire.AppHost;
using Microsoft.Extensions.Configuration;
using Projects;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        var configuration = builder.Configuration;
        var aspireWorkflow = configuration.GetValue<string>("ASPIRE_WORKFLOW");
        var sqlServerPassword = "Sample123";
        var sqlServerPasswordParameterResource =
            new ParameterResource(name: "sql-server-password", (_) => sqlServerPassword, secret: true);
        var sqlServerPasswordResource = builder.AddResource(sqlServerPasswordParameterResource);
        var isTestWorkflow = aspireWorkflow is AspireWorkflows.Test;
        int? sqlServerPort = isTestWorkflow ? null : AspireAppHostConfiguration.SqlContainerPort;

        var temporal = builder
            .AddTemporalServer(AspireAppHostConfiguration.TemporalServerResourceName);
        
        var sqlServer = builder
            .AddSqlServer(AspireAppHostConfiguration.SqlServerResourceName, sqlServerPasswordResource, sqlServerPort);
        
        // If running on macOS, use Azure SQL Edge image
        if (OperatingSystem.IsMacOS())
        {
            sqlServer.WithAnnotation(new ContainerImageAnnotation
            {
                Registry = "mcr.microsoft.com",
                Image = "azure-sql-edge",
                Tag = "latest"
            });
        }

        if (!isTestWorkflow)
        {
            sqlServer = sqlServer.WithDataVolume();
        }

        var db = sqlServer.AddDatabase(AspireAppHostConfiguration.SqlServerDatabaseName);

        builder.AddProject<Mews_Job_Scheduler_MigrationService>(AspireAppHostConfiguration.MigrationProjectResourceName)
            .WithReference(db)
            .WaitFor(sqlServer);

        builder.AddProject<Mews_Job_Scheduler>(AspireAppHostConfiguration.ProjectResourceName,
                "Mews.Job.Scheduler.Aspire")
            .WithReference(db)
            .WithReference(temporal)
            .WaitFor(sqlServer)
            .WaitFor(temporal);
        
        var app = builder.Build();
        app.Run();
    }
}

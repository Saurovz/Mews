using Microsoft.EntityFrameworkCore;
using TaxManager.Aspire.ServiceDefaults;
using TaxManager.Common.System;
using TaxManager.Configuration;
using TaxManager.Extensions;
using Serilog;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Services;
using TaxManager.EntityFrameworkCore.Data;
using TaxManager.Domain.Entities;

namespace TaxManager;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .ConfigureLogging()
                .CreateBootstrapLogger();

            Log.Information("Starting Web Host: TaxManager.Api, System Information: {@SystemInfo}",
                SystemHelper.GetSystemInfo());

            var app = BuildApplication(args);
            app.Run();

            Log.Information("Ending web host: TaxManager.Api");
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "Host terminated unexpectedly. An error occurred while starting the host!");
            Log.CloseAndFlush();
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static WebApplication BuildApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var environmentName = builder.Environment.EnvironmentName;

        builder.Host.UseSerilog((context, loggerConfiguration) => 
        {
            loggerConfiguration 
                .ReadFrom.Configuration(context.Configuration)
                .ConfigureLogging();
        });

        // ASPNETCORE_ENVIRONMENT = "localDev"
        if (environmentName.IsLocalDevelopment())
        {
            builder.AddSqlServerDbContext<AppDbContext>(connectionName :"taxmanager-local-db");
            builder.AddServiceDefaults();
        }
        
        var startup = new Startup(builder.Environment, builder.Configuration);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        startup.Configure(app);
        //This call may be used for quick db setup for test; else EF migration does the job
;       // startup.ConfigureDb(app);
        
        return app;
    }
}

using Microsoft.EntityFrameworkCore;
using TaxManager.DbMigrationService;
using TaxManager.EntityFrameworkCore.Data;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();


builder.Services.AddDbContextPool<AppDbContext>(options =>
    // options.UseSqlServer(builder.Configuration.GetConnectionString("sql-server"), sqlOptions => {
    //     // Workaround for https://github.com/dotnet/aspire/issues/1023
    //    // sqlOptions.ExecutionStrategy(c => new RetryingSqlServerRetryingExecutionStrategy(c));
    // }));
    options.UseSqlServer(
        connectionString:
        "Server=127.0.0.1,14353;User ID=sa;Password=Sample123;TrustServerCertificate=true;" +
        "Initial Catalog=taxmanager-local-db;")
        );
  
// builder.EnrichSqlServerDbContext<AppDbContext>(settings =>
//     // Disable Aspire default retries as we're using a custom execution strategy
//     settings.DisableRetry = true
// );

var host = builder.Build();
host.Run();

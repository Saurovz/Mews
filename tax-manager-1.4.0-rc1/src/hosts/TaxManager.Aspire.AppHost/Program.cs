using k8s.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using TaxManager.Aspire.AppHost;
using TaxManager.Aspire.AppHost.Extensions;


var builder = DistributedApplication.CreateBuilder(args);

//Use this to persist data across application-sessions
var isVolNeeded  = builder.Configuration.GetValue<bool>("UseVolume");

var sqlServer = builder.AddSqlServerWithOptionalVolume(
    name: AspireAppHostConfiguration.SqlServerResourceName,
    sqlServerInstancePort: OperatingSystem.IsWindows() ? AspireAppHostConfiguration.SqlContainerPort : 1433,
    useDataVolume: isVolNeeded,
    secret: "sql-password",
    volumeName: "localDataVolume");

var sqlDbase = sqlServer.AddDatabase(AspireAppHostConfiguration.SqlServerDatabaseName);

builder.AddProject<Projects.TaxManager_DbMigrationService>(AspireAppHostConfiguration.MigrationProjectResourceName)
    .WithReference(sqlDbase)
    .WaitFor(sqlServer);

var redis = builder.AddRedisCacheWithOptionalVolume(
    name: AspireAppHostConfiguration.RedisResourceName,
    useRedisCommander: true,
    redisCommanderPort: AspireAppHostConfiguration.RedisCommanderContainerPort,
    redisInstancePort: AspireAppHostConfiguration.RedisContainerPort,
    useDataVolume: false
);

var taxmanagerApi = builder.AddProject<Projects.TaxManager_Api>(
        AspireAppHostConfiguration.ProjectResourceName)
    .WithReference(sqlDbase)
    .WithReference(redis)
    .WaitFor(sqlDbase);

builder.AddNpmApp(AspireAppHostConfiguration.WebProjectResourceName, workingDirectory: "../TaxManager.Web")
    .WithReference(taxmanagerApi)
    .WithHttpEndpoint(env: "PORT", port: 3000, isProxied: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();

namespace TaxManager.Aspire.AppHost.Extensions;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<SqlServerServerResource> AddSqlServerWithOptionalVolume(
        this IDistributedApplicationBuilder builder,
        string name,
        int sqlServerInstancePort,
        bool useDataVolume,
        string secret,
        string? volumeName = null,
        string? databaseName = null)
    {
        var sqlPassword = builder.AddParameter(name:secret, secret: false);
        
        var sqlServer = builder.AddSqlServer(name: name,
            port: sqlServerInstancePort, password: sqlPassword);
        
        
        if (useDataVolume && !string.IsNullOrEmpty(volumeName))
        {
            sqlServer.WithDataVolume(name: volumeName);
        }
        
        //This is taken set outside as we need SlqServerDatabaseResource 
        // if (!string.IsNullOrEmpty(databaseName))
        // {
        //    sqlServer.AddDatabase(name: databaseName);
        // }

        return sqlServer;
    }

    public static IResourceBuilder<RedisResource> AddRedisCacheWithOptionalVolume(
        this IDistributedApplicationBuilder builder, 
        string name,
        bool useRedisCommander,
        int? redisInstancePort, int redisCommanderPort,
        bool useDataVolume,
        string? volumeName = null,
        string[]? arguments = null
    )
    {
        var redis =  builder.AddRedis(name: name);
        if (useRedisCommander) redis
              .WithRedisCommander(o=>
              {
                  o.WithHostPort(port: redisCommanderPort);
              });
        if (useDataVolume) redis.WithDataVolume();
        if (!(arguments == null || arguments.Length == 0)) redis.WithArgs(arguments);
        if (redisInstancePort != null) redis.WithEndpoint("tcp", e => e.Port = redisInstancePort);
          
       return redis;
                  
        //.WithArgs(["--maxmemory 500mb"]) //.WithArgs("--requirepass mypassword"); // ConnectionStr becomes "redis:port,password=mypassword"
        //.WithRedisCommander(); // This deploys a commander container(http://localhost:8081) alongside Redis giving a UI interface to see the cache-keys
        //.WithDataVolume()  // Persistent storage
        //.WithEndpoint("tcp", e => e.Port = 6380); 
       
    }
}

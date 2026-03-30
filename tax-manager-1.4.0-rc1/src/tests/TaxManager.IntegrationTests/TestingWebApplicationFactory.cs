using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Services;
using TaxManager.Configuration;
using TaxManager.Domain.Interfaces;

using TaxManager.EntityFrameworkCore.Data;
using TaxManager.EntityFrameworkCore.Persistence;

namespace TaxManager.IntegrationTests
{
    public sealed class TestingWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
    {
        private SqliteConnection _connection;
        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var server = base.CreateServer(builder);
            return server;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            
            base.ConfigureWebHost(builder);
            builder.UseEnvironment(SupportedEnvironments.LocalDevelopment);
            
            builder.ConfigureTestServices(services =>
            {
                //Aspire's DbContext registration removed
                var dbContextDescriptor = services
                    .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);
                
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection); // Use the same connection for all tests
                    options.EnableSensitiveDataLogging();
                });
                // Initialize database schema
                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
                DataSeeder.SeedData(dbContext);
                
                //ILoggerFactory is removed to address logger frozen issues
                var newLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
                services.RemoveAll<ILoggerFactory>();
                services.AddSingleton<ILoggerFactory>(newLoggerFactory);
            });
        }
    }
}

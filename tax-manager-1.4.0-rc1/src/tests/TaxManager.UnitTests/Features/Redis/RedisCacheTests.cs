using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Services;

namespace TaxManager.UnitTests.Features.Redis;


[TestFixture]
public class RedisCacheTests
{
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        var mock = new Mock<IDistributedCache>();
        mock.Setup(c => c.GetAsync("errorTest", new CancellationToken())).ThrowsAsync(new Exception());
        
        services.AddTransient<IDistributedCache>(sp => mock.Object);
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddLogging(l => l.AddFakeLogging());
        services.AddDistributedMemoryCache();

        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Test]
    public async Task Redis_Service_Exists_Logs_error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var cacheService = scopedServices.GetRequiredService<ICacheService>();
        var loggerCollector = scopedServices.GetRequiredService<FakeLogCollector>();
        

        var cacheResult = await cacheService.ExistsAsync("errorTest");
        Assert.That(cacheResult, Is.False);
        Assert.That(loggerCollector.LatestRecord.Message, Is.EqualTo("Redis exists operation failed for key errorTest"));

    }
}

using System.Net;
using Aspire.Hosting.Testing;
using Mews.Job.Scheduler.Aspire.AppHost;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.Health;

[TestFixture]
internal class ApplicationHealthCheckTests : TestBase
{
    [TestCase("/health")]
    [TestCase("/db/health")]
    public async Task Get_HealthCheckEndpoints_ReturnSuccessAndCorrectContentType(string endpoint)
    {
        // Arrange
        // Act
        var httpClient = IntegrationTests.AspireDistributedApp.CreateHttpClient(AspireAppHostConfiguration.ProjectResourceName);
        var response = await httpClient.GetAsync(endpoint);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}

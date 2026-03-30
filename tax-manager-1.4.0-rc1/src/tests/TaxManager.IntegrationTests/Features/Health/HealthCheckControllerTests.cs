using NUnit.Framework;
using System.Net;

namespace TaxManager.IntegrationTests.Features.Health;

internal class HealthCheckControllerTests
{
    [TestFixture]
    public class HealthTests : TestBase
    {
        [Test]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/health");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}

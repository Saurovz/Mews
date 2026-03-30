using System.Net;
using NUnit.Framework;
using TaxManager.Application.Common.Exception;
using TaxManager.Application.Dto;

namespace TaxManager.IntegrationTests.Features.Search;

internal class SearchControllerTests
{
    public class SearchTests : TestBase
    {
        [TestCase("CA")]
        public async Task Search_By_Code_Returns_Taxations(string code)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/Search/GetSearchResults?code={code}");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var results = response.Content.ReadAsAsync<SearchDto>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(results.Taxations, Is.Not.Null);
            Assert.That(results.Taxations.Any(), Is.True);
        }
        
        [TestCase("Andorra")]
        public async Task Search_By_Name_Returns_LegalEnvironments(string name)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/Search/GetSearchResults?name={name}");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var results = response.Content.ReadAsAsync<SearchDto>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(results.LegalEnvironments, Is.Not.Null);
            Assert.That(results.LegalEnvironments.Any(), Is.True);
        }
    }
}

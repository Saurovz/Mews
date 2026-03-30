using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TaxManager.Application.Dto;

namespace TaxManager.IntegrationTests.Features.Country;

internal class CountryControllerTests
{
    public class CountryTests : TestBase
    {
        [Test]
        public async Task ListCountries_Returns_Data()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/Country/ListCountries");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var countries = response.Content.ReadAsAsync<List<CountryDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(countries, Is.Not.Empty);
            Assert.That(countries, Has.Count.GreaterThan(0));
        }
        
        [TestCase(1)]
        public async Task GetSubdivisionsByCountryId_Returns_Data(int countryId)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/Country/GetSubdivisionsByCountryId/{countryId}");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var subdivisions = response.Content.ReadAsAsync<List<SubdivisionDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(subdivisions, Is.Not.Empty);
            Assert.That(subdivisions, Has.Count.GreaterThan(0));
        }
        
        [TestCase(99999)]
        public async Task GetSubdivisionsByCountryId_With_Invalid_CountryId_Returns_NotFound(int countryId)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/Country/GetSubdivisionsByCountryId/{countryId}");
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(result["error"].ToString(), Is.EqualTo("No Items found."));
        }
    }
}

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TaxManager.Application.Dto;
using TaxManager.EntityFrameworkCore.Data;
using StringContent = System.Net.Http.StringContent;

namespace TaxManager.IntegrationTests.Features.LegalEnvironment;

internal class LegalEnvironmentControllerTests
{
    [TestFixture]
    public class LegalEnvironmentTests : TestBase
    {
        [Test]
        public async Task ListLegalEnvironments_Returns_Data()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/LegalEnvironment/ListLegalEnvironments");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var legalEnvironments = response.Content.ReadAsAsync<List<LegalEnvironmentDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(legalEnvironments, Is.Not.Empty);
            Assert.That(legalEnvironments, Has.Count.GreaterThan(0));
        }
        
        [TestCase("AD")]
        public async Task Get_LegalEnvironment_By_Code_Returns_Data(string code)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/LegalEnvironment/{code}");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var legalEnvironment = response.Content.ReadAsAsync<LegalEnvironmentDto>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(legalEnvironment, Is.Not.Null);
            Assert.That(legalEnvironment.Code, Is.EqualTo(code));
        }
        
        [TestCase("ENV-1-Bad")]
        public async Task Get_LegalEnvironment_With_Bad_Code_Returns_No_Data_Message(string code)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/LegalEnvironment/{code}");

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(result["error"].ToString(), Is.EqualTo($"Entity with code '{code}' not found."));
        }
        
        [TestCase("ENV-1-New")]
        public async Task AddLegalEnvironment_Creates_New_Record(string code)
        {
            var client = _webApplicationFactory.CreateClient();
            var scope = _webApplicationFactory.Services.CreateScope();
            var context =  scope.ServiceProvider.GetService<AppDbContext>();
            var legalEnvironment = new JObject
            {
                { "code", code },
                { "name", "LegalEnvironment 1" },
                { "DepositTaxRateMode", 1},
                { "TaxationIds", new JArray(
                    context.Taxations.FirstOrDefault().Id
                )}
            };
            
            var httpContent = new StringContent(legalEnvironment.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/LegalEnvironment/AddLegalEnvironment", httpContent);

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<LegalEnvironmentDto>().Result;
            var addedItem = context.LegalEnvironments.SingleOrDefault(t => t.Code == code);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(addedItem.Name));
        }
        
        [TestCase("AD")]
        public async Task AddLegalEnvironment_With_Existing_Code_Returns_Bad_Request(string code)
        {
            var client = _webApplicationFactory.CreateClient();
            var legalEnvironment = new JObject
            {
                { "code", code },
                { "name", "Andorra" },
                { "DepositTaxRateMode", 1},
                { "TaxationIds", new JArray(
                    new Guid() 
                )}
            };
            
            var httpContent = new StringContent(legalEnvironment.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/LegalEnvironment/AddLegalEnvironment", httpContent);

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(result["error"].ToString(), Is.EqualTo("Legal Environment Code exists!!"));
        }
    }
}

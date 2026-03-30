using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TaxManager.Application.Dto;
using TaxManager.EntityFrameworkCore.Data;
using StringContent = System.Net.Http.StringContent;

namespace TaxManager.IntegrationTests.Features.Taxation;

internal class TaxationControllerTests
{
    [TestFixture]
    public class TaxationTests : TestBase
    {
        [Test]
        public async Task ListTaxations_Returns_Data()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/Taxation/ListTaxations");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var taxations = response.Content.ReadAsAsync<List<TaxationDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(taxations, Is.Not.Empty);
            Assert.That(taxations, Has.Count.GreaterThan(0));
        }
        
        [TestCase("CA-ON")]
        public async Task Get_Taxation_By_Code_Returns_Data(string code)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/Taxation/{code}");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var taxation = response.Content.ReadAsAsync<TaxationDto>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(taxation, Is.Not.Null);
            Assert.That(taxation.Code, Is.EqualTo(code));
        }
        
        [TestCase("US-TX-Bad")]
        public async Task Get_Taxation_With_Bad_Code_Returns_No_Data_Message(string code)
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync($"/Taxation/{code}");

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(result["error"].ToString(), Is.EqualTo($"Entity with code '{code}' not found."));
        }
        
        [TestCase("Tax-US-1")]
        public async Task AddTaxation_Creates_New_Record(string code)
        {
            var client = _webApplicationFactory.CreateClient();
            var scope = _webApplicationFactory.Services.CreateScope();
            var context =  scope.ServiceProvider.GetService<AppDbContext>();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", 6 },
                { "name", "US Taxation 1" },
                { "taxationTaxRates", new JArray( 
                    new JObject
                    {
                        {"taxRateId", 1},
                        {"strategyId", 1},
                        {"value", 10},
                        {"valueType", "USD"},
                        {"startDate", DateTime.Now},
                        {"endDate", DateTime.Now.AddDays(1)},
                        {"startDateTimeZone", "America/Chicago"},
                        {"endDateTimeZone", "America/Chicago"}
                    }
                )}
            };
            
            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<SaveValidationResultDto>().Result;
            var addedItem = context.Taxations.Include(t => t.TaxationTaxRates).SingleOrDefault(t => t.Code == code);
            var addedTaxRate = context.TaxationTaxRates.SingleOrDefault(s => s.TaxationId == addedItem.Id && s.TaxRateId == 1);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Entity, Is.Not.Null);
            var resultItem = JObject.Parse(result.Entity.ToString()).ToObject<TaxationDto>();
            if (addedItem != null)
            {
                Assert.That(resultItem.Name, Is.EqualTo(addedItem.Name));
                Assert.That(addedItem.TaxationTaxRates, Is.Not.Empty);
                Assert.That(addedItem.TaxationTaxRates, Has.Count.GreaterThan(0));
                Assert.That(addedItem.TaxationTaxRates.FirstOrDefault()?.Value, Is.EqualTo(10));
                Assert.That(addedTaxRate, Is.Not.Null);
                Assert.That(addedTaxRate.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(addedTaxRate.Value, Is.EqualTo(10));
            }
        }
        
        [TestCase("CA-ON")]
        public async Task AddTaxation_With_Existing_Code_Returns_Error(string code)
        {
            var client = _webApplicationFactory.CreateClient();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", 6 },
                { "name", "US Taxation 1" }
            };
            
            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);

            
            var result = response.Content.ReadAsAsync<SaveValidationResultDto>().Result;
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("Taxation Code exists!!"));
        }
        
        [TestCase("Tax-US-1", "Kansas")]
        public async Task AddTaxation_With_New_Subdivision_Creates_New_Records_And_Link(string code, string subdivisionName)
        {
            var client = _webApplicationFactory.CreateClient();
            var scope = _webApplicationFactory.Services.CreateScope();
            var context =  scope.ServiceProvider.GetService<AppDbContext>();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", 6 },
                { "name", "US Taxation 1" },
                { "subdivisions", new JArray( 
                    new JObject
                    {
                        { "id", "0" },
                        { "countryId", 6 },
                        { "name", subdivisionName }
                    }
                    )},
                { "taxationTaxRates", new JArray( 
                    new JObject
                    {
                        {"taxRateId", 1},
                        {"strategyId", 1},
                        {"value", 10},
                        {"valueType", "USD"},
                        {"startDate", DateTime.Now},
                        {"endDate", DateTime.Now.AddDays(1)},
                        {"startDateTimeZone", "America/Chicago"},
                        {"endDateTimeZone", "America/Chicago"}
                    }
                )}
            };
            
            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<SaveValidationResultDto>().Result;
            var addedItem = context.Taxations.Include(t => t.Subdivisions).SingleOrDefault(t => t.Code == code);
            var addedSubdivision = context.Subdivisions.SingleOrDefault(s => s.Name == subdivisionName);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Entity, Is.Not.Null);
            var resultItem = JObject.Parse(result.Entity.ToString()).ToObject<TaxationDto>();
            Assert.That(resultItem.Name, Is.EqualTo(addedItem.Name));
            Assert.That(resultItem.Subdivisions.First().Name, Is.EqualTo(addedSubdivision.Name));
            Assert.That(addedSubdivision.Id, Is.GreaterThan(0));
            Assert.That(addedItem.Subdivisions, Is.Not.Null);
            Assert.That(addedItem.Subdivisions.Count, Is.EqualTo(1));
        }
        
        [TestCase("Tax-US-1", 4)]
        public async Task AddTaxation_With_Existing_Subdivision_Creates_New_Record_and_Link(string code, int subdivisionId)
        {
            var client = _webApplicationFactory.CreateClient();
            var scope = _webApplicationFactory.Services.CreateScope();
            var context =  scope.ServiceProvider.GetService<AppDbContext>();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", 6 },
                { "name", "US Taxation 1" },
                { "subdivisions", new JArray( 
                    new JObject
                    {
                        { "id", subdivisionId },
                        { "countryId", 6 },
                        { "name", "California" }
                    }
                )},
                { "taxationTaxRates", new JArray( 
                    new JObject
                    {
                        {"taxRateId", 1},
                        {"strategyId", 1},
                        {"value", 10},
                        {"valueType", "USD"},
                        {"startDate", DateTime.Now},
                        {"endDate", DateTime.Now.AddDays(1)},
                        {"startDateTimeZone", "America/Chicago"},
                        {"endDateTimeZone", "America/Chicago"}
                    }
                )}
            };
            
            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<SaveValidationResultDto>().Result;
            var addedItem = context.Taxations.Include(t => t.Subdivisions).SingleOrDefault(t => t.Code == code);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Entity, Is.Not.Null);
            var resultItem = JObject.Parse(result.Entity.ToString()).ToObject<TaxationDto>();
            Assert.That(resultItem.Name, Is.EqualTo(addedItem.Name));
            Assert.That(addedItem.Subdivisions, Is.Not.Null);
            Assert.That(addedItem.Subdivisions.Count, Is.EqualTo(1));
            Assert.That(addedItem.Subdivisions.First().Id, Is.EqualTo(subdivisionId));
        }
        
        [TestCase("Tax-US-1", 999999)]
        public async Task AddTaxation_With_Invalid_Country_Returns_Bad_Request(string code, int countryId)
        {
            var client = _webApplicationFactory.CreateClient();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", countryId },
                { "name", "US Taxation 1" },
            };
            
            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(result["error"].ToString(), Is.EqualTo("Country id does not exist!"));
        }
        
        [TestCase("Tax-US-1", 999999)]
        public async Task AddTaxation_With_Invalid_Subdivision_Country_Returns_Bad_Request(string code, int countryId)
        {
            var client = _webApplicationFactory.CreateClient();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", 6 },
                { "name", "US Taxation 1" },
                { "subdivisions", new JArray( 
                    new JObject
                    {
                        { "id", 0 },
                        { "countryId", countryId },
                        { "name", "California" }
                    }
                )}
            };
            
            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(result["error"].ToString(), Is.EqualTo("Subdivision country id does not match Taxation."));
        }

        [TestCase("Tax-US-1", "California")]
        public async Task AddTaxation_With_New_Subdivision_With_Existing_Name_Returns_Error(string code,
            string subdivisionName)
        {
            var client = _webApplicationFactory.CreateClient();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", 238 },
                { "name", "US Taxation 1" },
                {
                    "subdivisions", new JArray(
                        new JObject
                        {
                            { "id", 0 },
                            { "countryId", 238 },
                            { "name", subdivisionName }
                        }
                    )
                },
                { "taxationTaxRates", new JArray( 
                    new JObject
                    {
                        {"taxRateId", 1},
                        {"strategyId", 1},
                        {"value", 10},
                        {"valueType", "USD"},
                        {"startDate", DateTime.Now},
                        {"endDate", DateTime.Now.AddDays(1)},
                        {"startDateTimeZone", "America/Chicago"},
                        {"endDateTimeZone", "America/Chicago"}
                    }
                )}
            };

            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);
            var result = response.Content.ReadAsAsync<SaveValidationResultDto>().Result;

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors,
            Does.Contain("Subdivision name already exists for the same country."));
        }
        
        [TestCase("AB")]
        public async Task AddTaxation_With_TaxRate_Code_Length_GreaterThan_One_Returns_Bad_Request(string code)
        {
            var client = _webApplicationFactory.CreateClient();
            var taxation = new JObject
            {
                { "code", code },
                { "countryId", 6 },
                { "name", "US Taxation 1" },
                { "taxationTaxRates", new JArray( 
                    new JObject
                    {
                        {"taxRateId", 1},
                        {"strategyId", 1},
                        {"code", code},
                        {"value", 10},
                        {"valueType", "USD"},
                        {"startDate", DateTime.Now},
                        {"endDate", DateTime.Now.AddDays(1)},
                        {"startDateTimeZone", "string"},
                        {"endDateTimeZone", "string"}
                    }
                )}

            };

            var httpContent = new StringContent(taxation.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/Taxation/AddTaxation", httpContent);
            var result = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());

            Assert.That(result.Errors.Last().Value.First(),
                Does.StartWith("The JSON value could not be converted to TaxManager.Application.Dto.TaxationTaxRateDto."));
        }

        [Test]
        public async Task GetTaxRates_Returns_Data()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/Taxation/GetTaxRates");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<List<TaxRateDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.GreaterThan(0));
        }
        
        [Test]
        public async Task GetStrategies_Returns_Data()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/Taxation/GetStrategies");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<List<StrategyDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.GreaterThan(0));
        }
        
        [Test]
        public async Task GetCurrencies_Returns_Data()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/Taxation/GetCurrencies");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<List<CurrencyDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.GreaterThan(0));
        }
        
        [Test]
        public async Task GetTimeZones_Returns_Data()
        {
            var client = _webApplicationFactory.CreateClient();

            var response = await client.GetAsync("/Taxation/GetTimeZones");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var result = response.Content.ReadAsAsync<List<TimeZoneDto>>().Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Count.GreaterThan(0));
        }
    }
}

using System.Text.Json;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Enums;
using TaxManager.EntityFrameworkCore.Data;

namespace TaxManager.EntityFrameworkCore.Data;

public static class DataSeeder
{
    public static void SeedData(AppDbContext context)
    {
        //TBD : log this operation
        // Add if data does not  exist
        var countries = AddCountries();
        if(!context.Countries.Any())
        {
            context.Countries.AddRange(countries);
            context.SaveChanges();
        }
        
        var subdivisions = new List<Subdivision>
        {
            new Subdivision { CountryId = countries.FirstOrDefault(c => c.Code == "CAN")!.Id, Name = "Quebec" },
            new Subdivision { CountryId = countries.FirstOrDefault(c => c.Code == "ASM")!.Id, Name = "Western" },
            new Subdivision { CountryId = countries.FirstOrDefault(c => c.Code == "ALA")!.Id, Name = "Aland" },
            new Subdivision { CountryId = countries.FirstOrDefault(c => c.Code == "USA")!.Id, Name = "California" },
            new Subdivision { CountryId = countries.FirstOrDefault(c => c.Code == "USA")!.Id, Name = "New York" }
        };
        if (!context.Subdivisions.Any())
        {
            context.Subdivisions.AddRange(subdivisions);
            context.SaveChanges();
        }

        var taxRates = new List<TaxRate>
        {
            new TaxRate { Name = "Accommodation" },
            new TaxRate { Name = "Cancellation Fee" },
            new TaxRate { Name = "Higher Increased" },
            new TaxRate { Name = "Highest Increased" },
            new TaxRate { Name = "Increased" },
            new TaxRate { Name = "Lower Reduced" },
            new TaxRate { Name = "Lowest Reduced" },
            new TaxRate { Name = "Maximum" },
            new TaxRate { Name = "Minimum" },
            new TaxRate { Name = "No Show Fee" },
            new TaxRate { Name = "Platform" },
            new TaxRate { Name = "Reduced" },
            new TaxRate { Name = "Standard" },
            new TaxRate { Name = "Zero" }
        };
        if (!context.TaxRates.Any())
        {
            context.TaxRates.AddRange(taxRates);
            context.SaveChanges();
        }
        var taxations = new List<Taxation>
        {
            new Taxation { Code = "CA-ON", CountryId = countries.FirstOrDefault(c => c.Code == "CAN")!.Id, Name = "CA subdivision"   },
            new Taxation { Code = "CH-2018", CountryId = countries.FirstOrDefault(c => c.Code == "ASM")!.Id, Name = "Taxation flat rate" },
            new Taxation { Code = "ES-CAN-2020", CountryId = countries.FirstOrDefault(c => c.Code == "ALA")!.Id, Name = "US subdivision" },
            new Taxation { Code = "US-CA-TE", CountryId = countries.FirstOrDefault(c => c.Code == "USA")!.Id, Name = "Taxation relative" },
            new Taxation { Code = "US-CA-TE-TE", CountryId = countries.FirstOrDefault(c => c.Code == "USA")!.Id, Name = "Taxation with dependent" },
            new Taxation { Code = "US-CA-TE-TE-1", CountryId = countries.FirstOrDefault(c => c.Code == "USA")!.Id, Name = "Taxation with two level dependents" }
        };
        //Add subdivision links
        taxations.First(t => t.Code == "CA-ON")
            .Subdivisions.Add(subdivisions.First(s => s.CountryId == countries.FirstOrDefault(c => c.Code == "CAN")!.Id));
        taxations.First(t => t.Code == "ES-CAN-2020")
            .Subdivisions.AddRange(subdivisions.Where(s => s.CountryId == countries.FirstOrDefault(c => c.Code == "USA")!.Id));
        context.Taxations.AddRange(taxations);
        context.SaveChanges();
        //Add tax rate links
        context.Taxations.First(t => t.Code == "CH-2018").TaxationTaxRates.Add(
            new TaxationTaxRate { TaxRateId = 9, Strategy = Strategy.FlatRate, Code = 'X', Value = 543.21, ValueType = "CAD", 
                StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago",
                StartDate = new DateTime(2025, 06, 01, 00, 00, 00), 
                EndDate = new DateTime(2025, 12, 31, 00, 00, 00)
            });
        
        context.Taxations.First(t => t.Code == "US-CA-TE").TaxationTaxRates.Add(
            new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRate, Code = 'X', Value = 2, ValueType = "%", 
                StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago",
                StartDate = new DateTime(2025, 06, 01, 00, 00, 00), 
                EndDate = new DateTime(2025, 12, 31, 00, 00, 00)
            });
        
        context.Taxations.First(t => t.Code == "US-CA-TE-TE").TaxationTaxRates.Add(
            new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRateWithDependencies, 
                Code = 'X', Value = 1.5, ValueType = "%", StartDateTimeZone = "", EndDateTimeZone = "",
            StartDate = null, EndDate = null
        });
        context.SaveChanges();
        //Dependent taxations
        context.Taxations.First(t => t.Code == "US-CA-TE-TE").TaxationTaxRates.First().DependentTaxations = new 
            List<DependentTaxation>()
            {
                new() { ChildTaxationId = context.Taxations.First(t => t.Code == "US-CA-TE").Id }
            };

        context.SaveChanges();
        
        context.Taxations.First(t => t.Code == "US-CA-TE-TE-1").TaxationTaxRates.Add(
            new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRateWithDependencies, 
                Code = 'X', Value = 1.5, ValueType = "%", StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago",
                StartDate = new DateTime(2025, 05, 15, 00, 00, 00), 
                EndDate = new DateTime(2026, 2, 15, 00, 00, 00),
            });
        context.SaveChanges();
        //Dependent taxations
        context.Taxations.First(t => t.Code == "US-CA-TE-TE-1").TaxationTaxRates.First().DependentTaxations = new 
            List<DependentTaxation>()
            {
                new() { ChildTaxationId = context.Taxations.First(t => t.Code == "US-CA-TE-TE").Id }
            };

        context.SaveChanges();
        
        if (!context.LegalEnvironments.Any())
        {   
            var leTaxations = context.Taxations.Where(t => t.Code == "US-CA-TE").ToList();
            var legalEnvironments = new List<LegalEnvironment>
            {
                new LegalEnvironment() { Code = "AD", Name = "Andorra", DepositTaxRateMode = 0},
                new LegalEnvironment() { Code = "AR", Name = "Argentina", DepositTaxRateMode = 1},
                new LegalEnvironment() { Code = "AW-2023", Name = "Aruba", DepositTaxRateMode = 2},
                new LegalEnvironment() { Code = "AU", Name = "Australia", DepositTaxRateMode = 3},
                new LegalEnvironment() { Code = "US-AL-CH-CE", 
                    Name = "United States - Alabama State - Cherokee County - Centre", DepositTaxRateMode = 1, 
                    Taxations = leTaxations}
            };
            context.LegalEnvironments.AddRange(legalEnvironments);
            context.SaveChanges();
        }
      
        
        
        // If there are pending changes to save
        if (context.ChangeTracker.HasChanges())
        {
            context.SaveChanges();
        }
    }

    #region Private Static Methods
    private static List<Country>? AddCountries()
    {
        try
        {
            // Get the build output path
            var outputPath = AppDomain.CurrentDomain.BaseDirectory;
            // Pick the file  json file
            var dirPath = Path.Combine(outputPath, "Data/countries.json");
            // Check if file exists first
            if (!File.Exists(dirPath))
                throw new FileNotFoundException($"The specified file was not found: {dirPath}");
        
            var json =  File.ReadAllText(dirPath);
        
            //Check if json file is empty
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidDataException("The JSON file is empty");
            //to accept the json values irrespective of cases
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            var countries = JsonSerializer.Deserialize<List<Country>>(json, options);

            if (countries == null)
            {
                throw new InvalidDataException("Failed to deserialize the JSON data");
            }
            return countries;
        }
        catch (JsonException jsonEx)
        {
            throw new InvalidDataException("Invalid JSON format in the file", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception($"An unexpected error occurred while processing the file: {ex.Message}", ex);
        }
    }
    #endregion
  
}

namespace TaxManager.Application.Common;

public static class CacheKeys
{
    // Countries
    public const string CountryAll = "countries:all";   //implicitly static
    public static string CountryById(int id) => $"countries:{id}";
    public static string CountryByCode(string code) => $"countries:{code}";

    
    //Taxation
    public const string TaxationAll = "taxations:all";
    public static string TaxationByCode(string code) => $"taxations{code}";
    public static string TaxationsByTaxRate(int rateId) => $"taxations:rate:{rateId}"; 
    public static string TaxationByCountryId(int countryId) => $"taxations:country:{countryId}";
    public const string TaxRatesAll = "taxRates:all";
    public const string TimeZonesAll = "timeZones:all";
    
    //Search
    public static string SearchByCodeAndName(string code, string name) => $"search:{code}:{name}";
}

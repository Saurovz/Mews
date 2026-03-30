using TaxManager.Application.Dto;
using TaxManager.Domain.Entities;

namespace TaxManager.Application.Interfaces;

public interface ITaxationService
{
    Task<TaxationDto> GetTaxationByCodeAsync(string code);
    Task<IEnumerable<TaxationDto>> GetAllTaxationsAsync();
    Task<SaveValidationResultDto> CreateTaxationAsync(TaxationCreateDto taxationCreateDto);
    Task<IEnumerable<TaxRateDto>> GetTaxRatesAsync();
    IEnumerable<StrategyDto> GetStrategies();
    IEnumerable<CurrencyDto> GetCurrencies();
    Task<IEnumerable<TimeZoneDto>> GetTimeZonesAsync();
    Task<IEnumerable<TaxationDto>> GetTaxationsByCountryIdAsync(int countryId);
    // Task UpdateTaxationAsync(string code, TaxationDto taxationDto);
    // Task DeleteTaxationAsync(string code);
}

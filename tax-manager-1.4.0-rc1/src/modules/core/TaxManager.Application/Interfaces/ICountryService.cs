using TaxManager.Application.Dto;

namespace TaxManager.Application.Interfaces;

public interface ICountryService
{
    Task<IEnumerable<CountryDto>> GetAllCountriesAsync();
    Task<IEnumerable<SubdivisionDto>> GetSubdivisionsByCountryIdAsync(int countryId);
    Task<bool> DeleteSubdivisionByIdAsync(int id);
}

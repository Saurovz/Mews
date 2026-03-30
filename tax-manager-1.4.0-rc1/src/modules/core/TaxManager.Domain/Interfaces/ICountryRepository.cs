using TaxManager.Domain.Entities;

namespace TaxManager.Domain.Interfaces;

public interface ICountryRepository
{
    Task<IEnumerable<Country>> GetAllAsync();
    Task<Country?> GetCountryByIdAsync(int countryId);
    Task<Subdivision> GetSubdivisionByIdAsync(int id);
    Task<bool> CheckIfSubdivisionExistsAsync(int countryId, string name);
    Task<int> DeleteSubdivisionAsync(Subdivision subdivision);
    Task<IEnumerable<Subdivision>> GetSubdivisionsByCountryIdAsync(int countryId);
}

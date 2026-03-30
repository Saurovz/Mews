using TaxManager.Domain.Entities;
using System.Linq.Expressions;

namespace TaxManager.Domain.Interfaces;

public interface ITaxationRepository
{
    Task<IEnumerable<Taxation>> GetAllAsync();
    
    Task<Taxation?> GetByCodeAsync(string code);
    
    Task<Taxation?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// This method returns just the taxation entity, not it's associated entities
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Taxation?> GetByIdSimplifiedAsync(Guid id);

    Task<Taxation?> AddAsync(Taxation taxation);

    Task<bool> AnyAsync(Expression<Func<Taxation, bool>> predicate);
    
    Task<IEnumerable<TaxRate>> GetTaxRatesAsync();
    
    Task<TaxationTaxRate?> GetTaxationTaxRateAndDependentsTaxRates(Guid taxationId, int taxRateId);
    
    Task<IEnumerable<TaxationTaxRate?>> GetTaxationTaxRateAndDependeeTaxRates(Guid taxationId, int taxRateId);
    
    Task<IEnumerable<Taxation>> GetTaxationsByCountryIdAsync(int countryId);
    

}

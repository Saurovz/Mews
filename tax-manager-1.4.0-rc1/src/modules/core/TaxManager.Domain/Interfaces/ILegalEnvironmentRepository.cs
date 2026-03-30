using System.Linq.Expressions;
using TaxManager.Domain.Entities;

namespace TaxManager.Domain.Interfaces;

public interface ILegalEnvironmentRepository
{
    Task<IEnumerable<LegalEnvironment>> GetAllAsync();
    Task<LegalEnvironment?> GetByCodeAsync(string code);
    Task<LegalEnvironment?> AddAsync(LegalEnvironment legalEnvironment);
    Task<bool> AnyAsync(Expression<Func<LegalEnvironment, bool>> predicate);
}

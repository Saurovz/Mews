using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaxManager.Domain.Interfaces;
using TaxManager.Domain.Entities;
using TaxManager.EntityFrameworkCore.Data;

namespace TaxManager.EntityFrameworkCore.Persistence;

public class LegalEnvironmentRepository(AppDbContext context) : ILegalEnvironmentRepository
{
    public async Task<IEnumerable<LegalEnvironment>> GetAllAsync()
    {
        return await context.LegalEnvironments
            .Include(l => l.Taxations)
            .OrderBy(t => t.Code).ToListAsync();   
    }

    public async Task<LegalEnvironment?> GetByCodeAsync(string code)
    {
        return await context.LegalEnvironments
                .Include(l => l.Taxations)
                .FirstOrDefaultAsync(t => t.Code == code);
    }
    public async Task<LegalEnvironment?> AddAsync(LegalEnvironment legalEnvironment)
    {
        context.LegalEnvironments.Add(legalEnvironment);
        await context.SaveChangesAsync();
        return legalEnvironment;
    }
    public async Task<bool> AnyAsync(Expression<Func<LegalEnvironment, bool>> predicate)
    {
        return await context.LegalEnvironments.AnyAsync(predicate);
    }
}

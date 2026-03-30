using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaxManager.Domain.Interfaces;
using TaxManager.Domain.Entities;
using TaxManager.EntityFrameworkCore.Data;

namespace TaxManager.EntityFrameworkCore.Persistence;

public class TaxationRepository(AppDbContext context) : ITaxationRepository
{
    public async Task<IEnumerable<Taxation>> GetAllAsync()
    {
        return await context.Taxations
            .Include(t => t.Country)
            .Include(t => t.Subdivisions)
            .Include(t => t.TaxationTaxRates)
            .ThenInclude(tt => tt.DependentTaxations)
            .OrderBy(t => t.Code).ToListAsync();   
    }

    public async Task<Taxation?> GetByCodeAsync(string code)
    {
        var taxation = await context.Taxations
            .Include(t => t.Country)
            .Include(t => t.Subdivisions)
            .Include(t => t.TaxationTaxRates)
            .ThenInclude(tt => tt.DependentTaxations)
            .ThenInclude(dt => dt.ChildTaxation)
            .FirstOrDefaultAsync(t => t.Code == code);

        if (taxation?.Subdivisions != null)
        {
            taxation.Subdivisions = taxation.Subdivisions.OrderBy(s => s.Name).ToList();
        }

        return taxation;
    }
    
    public async Task<Taxation?> GetByIdAsync(Guid id)
    {
        var taxation = await context.Taxations
            .Include(t => t.Country)
            .Include(t => t.Subdivisions)
            .Include(t => t.TaxationTaxRates)
            .ThenInclude(tt => tt.DependentTaxations)
            .ThenInclude(dt => dt.ChildTaxation)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (taxation?.Subdivisions != null)
        {
            taxation.Subdivisions = taxation.Subdivisions.OrderBy(s => s.Name).ToList();
        }

        return taxation;
    }

    public async Task<Taxation?> GetByIdSimplifiedAsync(Guid id)
    {
        return await context.Taxations
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Taxation?> AddAsync(Taxation taxation)
    {
        context.ChangeTracker.TrackGraph(taxation, e =>
        {
            e.Entry.State = e.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
        });
        context.Taxations.Add(taxation);
        await context.SaveChangesAsync();
        return context.Taxations
            .Include(t => t.Country)
            .Include(t => t.Subdivisions)
            .Include(t => t.TaxationTaxRates)
            .ThenInclude(tt => tt.DependentTaxations)
            .ThenInclude(dt => dt.ChildTaxation)
            .FirstOrDefault(t => t.Code == taxation.Code);
    }
    public async Task<bool> AnyAsync(Expression<Func<Taxation, bool>> predicate)
    {
        return await context.Taxations.AnyAsync(predicate);
    }

    public async Task<IEnumerable<TaxRate>> GetTaxRatesAsync()
    {
        return await context.TaxRates.OrderBy(t => t.Id).ToListAsync();
    }

    public async Task<TaxationTaxRate?> GetTaxationTaxRateAndDependentsTaxRates(Guid taxationId, int taxRateId)
    {
        return await context.TaxationTaxRates
            .Include(t => t.Taxation)
            .Include(t => t.DependentTaxations)
            .ThenInclude(dt => dt.ChildTaxation)
            .ThenInclude(ct => ct.TaxationTaxRates.Where(cttr => cttr.TaxRateId == taxRateId))
            .FirstOrDefaultAsync(t => t.TaxationId == taxationId && t.TaxRateId == taxRateId);
    }

    public async Task<IEnumerable<TaxationTaxRate?>> GetTaxationTaxRateAndDependeeTaxRates(Guid taxationId, int taxRateId)
    {
        return await context.TaxationTaxRates
                .Where(t => t.TaxRateId == taxRateId && t.DependentTaxations.Any(dt => dt.ChildTaxationId == taxationId))
                .Include(t => t.Taxation)
                .ToListAsync();
    }

    public async Task<IEnumerable<Taxation>> GetTaxationsByCountryIdAsync(int countryId)
    {
        return await context.Taxations
            .Include(t => t.Subdivisions)
            .Where(t => t.CountryId == countryId).ToListAsync();
    }
}

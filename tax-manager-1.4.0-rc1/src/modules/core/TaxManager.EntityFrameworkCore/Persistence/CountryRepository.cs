using Microsoft.EntityFrameworkCore;
using TaxManager.Domain.Interfaces;
using TaxManager.Domain.Entities;
using TaxManager.EntityFrameworkCore.Data;

namespace TaxManager.EntityFrameworkCore.Persistence;

public class CountryRepository(AppDbContext context) : ICountryRepository
{
    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        return await context.Countries.OrderBy(t => t.Code).ToListAsync();   
    }

    public async Task<Country?> GetCountryByIdAsync(int countryId)
    {
        return await context.Countries.FirstOrDefaultAsync(c => c.Id == countryId);
    }

    public async Task<Subdivision> GetSubdivisionByIdAsync(int id)
    {
        return await context.Subdivisions.FirstAsync(s => s.Id == id);  
    }

    public async Task<bool> CheckIfSubdivisionExistsAsync(int countryId, string name)
    {
        return await context.Subdivisions.AnyAsync(s => s.CountryId == countryId && s.Name == name);
    }

    public async Task<IEnumerable<Subdivision>> GetSubdivisionsByCountryIdAsync(int countryId)
    {
        return await context.Subdivisions.Where(s => s.CountryId == countryId).ToListAsync();
    }

    public async Task<int> DeleteSubdivisionAsync(Subdivision subdivision)
    {
        context.Subdivisions.Remove(subdivision);
        return await context.SaveChangesAsync();
    }
}

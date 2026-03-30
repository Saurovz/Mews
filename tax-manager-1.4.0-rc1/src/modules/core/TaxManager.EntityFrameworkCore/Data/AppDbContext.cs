using Microsoft.EntityFrameworkCore;
using TaxManager.Domain.Entities;


namespace TaxManager.EntityFrameworkCore.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Country> Countries => Set<Country>(); 
        
        public DbSet<Taxation> Taxations => Set<Taxation>();
        
        public DbSet<LegalEnvironment> LegalEnvironments => Set<LegalEnvironment>();
        
        public DbSet<Subdivision> Subdivisions => Set<Subdivision>();
        
        public DbSet<TaxRate> TaxRates => Set<TaxRate>();
        
        public DbSet<TaxationTaxRate> TaxationTaxRates => Set<TaxationTaxRate>();
    }
}

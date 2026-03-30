using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaxManager.Domain.Entities;

[Index(nameof(Code), IsUnique = true)]
public class Taxation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 1)]
    public Guid Id { get; set; }
    
    [Required]
    [Column(TypeName = "varchar(40)")] //Initial value, length of properties may change.
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public int CountryId { get; set; }
    
    [Required]
    [Column(TypeName = "varchar(255)")]
    public string Name { get; set; } = string.Empty;
    
    [ForeignKey("CountryId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Country Country { get; set; }
    
    public virtual List<Subdivision> Subdivisions { get; set; }= new List<Subdivision>();

    public virtual List<TaxationTaxRate> TaxationTaxRates { get; set; } = new List<TaxationTaxRate>();
    
    public virtual IEnumerable<LegalEnvironment> LegalEnvironments { get; set; }
    
}

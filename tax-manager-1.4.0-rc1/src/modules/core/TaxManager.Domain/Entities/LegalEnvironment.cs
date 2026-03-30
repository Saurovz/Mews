using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaxManager.Domain.Entities
{
    [Index(nameof(Code), IsUnique = true)]
    public class LegalEnvironment
    {    
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public Guid Id { get; set; }
        [Required]
        [Column(TypeName = "varchar(40)")] //Initial value, length of properties may change.
        public string Code { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "varchar(70)")] //Initial value, length of properties may change.
        public string Name { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(1)")]
        public int DepositTaxRateMode { get; set; } = 0;
        
        public virtual List<Taxation> Taxations { get; set; } = new List<Taxation>();
        
        
    }
}

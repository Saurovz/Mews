using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaxManager.Domain.Entities;

[PrimaryKey(nameof(TaxationTaxRateId), nameof(ChildTaxationId))]
public class DependentTaxation
{
    [Required]
    public Guid TaxationTaxRateId { get; set; }
    [Required]
    public Guid ChildTaxationId { get; set; }
    
    [ForeignKey("ChildTaxationId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Taxation ChildTaxation { get; set; }
    
    [ForeignKey("TaxationTaxRateId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual TaxationTaxRate TaxationTaxRate { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TaxManager.Domain.Enums;

namespace TaxManager.Domain.Entities;

public class TaxationTaxRate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaxationId { get; set; }
    
    [Required]
    public int TaxRateId { get; set; }
    
    [Required]
    public Strategy Strategy { get; set; }
    
    [Column(TypeName = "char(1)")]
    public char? Code { get; set; }
    
    [Column(TypeName = "decimal(6,2)")]
    public double? Value { get; set; }

    [Column(TypeName = "varchar(10)")]
    public string? ValueType { get; set; } = string.Empty;
    
    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }
    
    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }
    
    [Column(TypeName = "varchar(50)")]
    public string? StartDateTimeZone { get; set; }
    
    [Column(TypeName = "varchar(50)")]
    public string? EndDateTimeZone { get; set; }
    
    public virtual List<DependentTaxation> DependentTaxations { get; set; } = null;
    
    [ForeignKey("TaxRateId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual TaxRate TaxRate { get; set; }
    
    [ForeignKey("TaxationId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Taxation Taxation { get; set; }
}

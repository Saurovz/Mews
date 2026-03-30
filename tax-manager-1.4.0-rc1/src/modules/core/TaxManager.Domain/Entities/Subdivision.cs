using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaxManager.Domain.Entities;

public class Subdivision
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [Column(TypeName = "varchar(50)")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int CountryId { get; set; }
    
    //Needed for EF many-to-many
    public virtual IEnumerable<Taxation> Taxations { get; set; }
    
    
    [ForeignKey("CountryId")]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Country Country { get; set; }
}

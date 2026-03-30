using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxManager.Domain.Entities
{
    public sealed class Country
    {    
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "char(3)")] //Initial value, length of properties may change.
        public string Code { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "varchar(50)")] //Initial value, length of properties may change.
        public string Name { get; set; } = string.Empty;
    }
}

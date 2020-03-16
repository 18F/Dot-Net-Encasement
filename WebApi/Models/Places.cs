using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    [Table("places")]
    public partial class Places
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column(TypeName = "decimal(38, 0)")]
        public decimal LatD { get; set; }
        [Column(TypeName = "decimal(38, 0)")]
        public decimal LatM { get; set; }
        [Column(TypeName = "decimal(38, 0)")]
        public decimal LatS { get; set; }
        [Required]
        [Column("NS")]
        public string Ns { get; set; }
        [Column(TypeName = "decimal(38, 0)")]
        public decimal LonD { get; set; }
        [Column(TypeName = "decimal(38, 0)")]
        public decimal LonM { get; set; }
        [Column(TypeName = "decimal(38, 0)")]
        public decimal LonS { get; set; }
        [Required]
        [Column("EW")]
        public string Ew { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
    }
}

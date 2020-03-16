using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    [Table("inspections")]
    public partial class Inspections
    {
        [Column(TypeName = "numeric")]
        public decimal ScoreRecent { get; set; }
        [Required]
        [Column(TypeName = "character varying")]
        public string GradeRecent { get; set; }
        public DateTime? DateRecent { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? Score2 { get; set; }
        [Column(TypeName = "character varying")]
        public string Grade2 { get; set; }
        public DateTime? Date2 { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? Score3 { get; set; }
        [Column(TypeName = "character varying")]
        public string Grade3 { get; set; }
        public DateTime? Date3 { get; set; }
        [Key]
        [Column("permit_number", TypeName = "numeric")]
        public decimal PermitNumber { get; set; }
        [Column("facility_type", TypeName = "numeric")]
        public decimal FacilityType { get; set; }
        [Required]
        [Column("facility_type_description", TypeName = "character varying")]
        public string FacilityTypeDescription { get; set; }
        [Column("subtype", TypeName = "numeric")]
        public decimal Subtype { get; set; }
        [Required]
        [Column("subtype_description", TypeName = "character varying")]
        public string SubtypeDescription { get; set; }
        [Required]
        [Column("premise_name", TypeName = "character varying")]
        public string PremiseName { get; set; }
        [Required]
        [Column("premise_address", TypeName = "character varying")]
        public string PremiseAddress { get; set; }
        [Required]
        [Column("premise_city", TypeName = "character varying")]
        public string PremiseCity { get; set; }
        [Required]
        [Column("premise_state", TypeName = "character varying")]
        public string PremiseState { get; set; }
        [Column("premise_zip", TypeName = "numeric")]
        public decimal PremiseZip { get; set; }
        [Column("opening_date")]
        public DateTime? OpeningDate { get; set; }
    }
}

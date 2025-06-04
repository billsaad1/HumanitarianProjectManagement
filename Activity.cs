using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models {
    [Table("Activities")]
    public class Activity {
        [Key]
        public int ActivityID { get; set; }

        // [Required] // This will need to be removed if OutputID becomes nullable - REMOVED
        public int? OutputID { get; set; } // Made nullable

        public int? OutcomeID { get; set; } // Added OutcomeID

        [Required]
        public string ActivityDescription { get; set; }

        public string PlannedMonths { get; set; } // For activity planning table

        [ForeignKey("OutcomeID")]
        public virtual Outcome Outcome { get; set; }

        [ForeignKey("OutputID")]
        public virtual Output Output { get; set; }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HumanitarianProjectManagement.Models;

namespace HumanitarianProjectManagement.Models {
    [Table("Outcomes")]
    public class Outcome {
        [Key]
        public int OutcomeID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [Required]
        public string OutcomeDescription { get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        public virtual ICollection<Output> Outputs { get; set; }
        // public object Activities { get; internal set; } // Removed this ambiguous property

        public Outcome()
        {
            Outputs = new HashSet<Output>();
        }
    }
}

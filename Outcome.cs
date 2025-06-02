using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HumanitarianProjectManagement.Models; // Added to ensure Project and Output are found if they are in this namespace explicitly

namespace HumanitarianProjectManagement.Models
{
    [Table("Outcomes")]
    public class Outcome
    {
        [Key]
        public int OutcomeID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [Required]
        public string OutcomeDescription { get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        public virtual ICollection<Output> Outputs { get; set; }

        public Outcome()
        {
            Outputs = new HashSet<Output>();
        }
    }
}

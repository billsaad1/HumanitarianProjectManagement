using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HumanitarianProjectManagement.Models;

namespace HumanitarianProjectManagement.Models {
    [Table("Outputs")]
    public class Output {
        [Key]
        public int OutputID { get; set; }

        [Required]
        public int OutcomeID { get; set; }

        [Required]
        public string OutputDescription { get; set; }

        [ForeignKey("OutcomeID")]
        public virtual Outcome Outcome { get; set; }

        public virtual ICollection<ProjectIndicator> ProjectIndicators { get; set; }
        public virtual ICollection<Activity> Activities { get; set; }

        public Output()
        {
            ProjectIndicators = new HashSet<ProjectIndicator>();
            Activities = new HashSet<Activity>();
        }
    }
}

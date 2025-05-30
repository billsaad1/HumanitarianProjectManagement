using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models
{
    [Table("ProjectIndicators")]
    public class ProjectIndicator
    {
        [Key]
        public int IndicatorID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Indicator name is required.")]
        [DisplayName("Indicator Name")]
        public string IndicatorName { get; set; } // NVARCHAR(MAX)

        [DisplayName("Description")]
        public string Description { get; set; } // NVARCHAR(MAX)

        [StringLength(255)]
        [DisplayName("Target Value")]
        public string TargetValue { get; set; }

        [StringLength(255)]
        [DisplayName("Actual Value")]
        public string ActualValue { get; set; }

        [StringLength(100)]
        [DisplayName("Unit of Measure")]
        public string UnitOfMeasure { get; set; }

        [StringLength(255)]
        [DisplayName("Baseline Value")]
        public string BaselineValue { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [DisplayName("Is Key Indicator")]
        public bool IsKeyIndicator { get; set; } = false;

        // Navigation properties
        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }
        public virtual ICollection<VerificationMean> VerificationMeans { get; set; }

        public ProjectIndicator()
        {
            VerificationMeans = new HashSet<VerificationMean>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models {
    [Table("ProjectIndicators")]
    public class ProjectIndicator {
        [Key]
        public int ProjectIndicatorID { get; set; } // Changed from IndicatorID

        [Required]
        public int ProjectID { get; set; }

        public int? OutputID { get; set; } // Foreign Key to Output, nullable
        public int? OutcomeID { get; set; } // Added OutcomeID

        [Required(ErrorMessage = "Indicator name is required.")]
        [DisplayName("Indicator Name")]
        public string IndicatorName { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [StringLength(255)]
        [DisplayName("Overall Target Value (Textual)")]
        public string TargetValue { get; set; } // This was the original target field

        // New Target Fields from issue:
        [DisplayName("Target Men")]
        public int TargetMen { get; set; }

        [DisplayName("Target Women")]
        public int TargetWomen { get; set; }

        [DisplayName("Target Boys")]
        public int TargetBoys { get; set; }

        [DisplayName("Target Girls")]
        public int TargetGirls { get; set; }

        [DisplayName("Target Total (Calculated or User-Entered)")]
        public int TargetTotal { get; set; }
        // End New Target Fields

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

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        [ForeignKey("OutputID")]
        public virtual Output Output { get; set; }

        [ForeignKey("OutcomeID")] // Added this FK
        public virtual Outcome Outcome {get; set; }


        public virtual ICollection<VerificationMean> VerificationMeans { get; set; }

        public ProjectIndicator()
        {
            VerificationMeans = new HashSet<VerificationMean>();
            // Initialize new target fields
            TargetMen = 0;
            TargetWomen = 0;
            TargetBoys = 0;
            TargetGirls = 0;
            TargetTotal = 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models
{
    [Table("Activities")]
    public class Activity
    {
        [Key]
        public int ActivityID { get; set; }

        [Required]
        public int BudgetID { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [StringLength(255)]
        [DisplayName("Activity Name")]
        public string ActivityName { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; } // NVARCHAR(MAX)

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayName("Estimated Cost")]
        public decimal EstimatedCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayName("Actual Cost")]
        public decimal? ActualCost { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [StringLength(255)]
        public string Location { get; set; }

        [DisplayName("Notes")]
        public string Notes { get; set; } // NVARCHAR(MAX)

        // Navigation property
        [ForeignKey("BudgetID")]
        public virtual Budget Budget { get; set; }

        [Required]
        public int OutputID { get; set; }

        [ForeignKey("OutputID")]
        public virtual Output Output { get; set; }

        // Navigation property for StockTransactions
        public virtual ICollection<StockTransaction> StockTransactions { get; set; }

        public Activity()
        {
            StockTransactions = new HashSet<StockTransaction>();
        }
    }
}

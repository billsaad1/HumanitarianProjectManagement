using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Ensure this using directive is present if BudgetCategoriesEnum is in a different namespace,
// but given the plan, it should be in HumanitarianProjectManagement.Models as well.
// using HumanitarianProjectManagement.Models;

namespace HumanitarianProjectManagement.Models
{
    public class DetailedBudgetLine
    {
        [Key]
        public int DetailedBudgetLineID { get; set; } // Changed from Id to DetailedBudgetLineID

        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [Required]
        public BudgetCategoriesEnum Category { get; set; } // Using the new enum

        [StringLength(1500)]
        public string Description { get; set; } // Remarks (max 1500 characters)

        [StringLength(50)]
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Duration { get; set; } // e.g., number of months, days, etc.
        public decimal PercentageChargedToCBPF { get; set; } // Store as 0-100
        public decimal TotalCost { get; set; } // Calculated: Quantity * UnitCost * Duration * (%ChargedToCBPF / 100)
    }
}

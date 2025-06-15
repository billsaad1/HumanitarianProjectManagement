using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel; // Required for BindingList
using System.Collections.Generic; // Required for List, though we are changing one
using HumanitarianProjectManagement; // Added for BudgetSubCategory

// Ensure this using directive is present if BudgetCategoriesEnum is in a different namespace,
// but given the plan, it should be in HumanitarianProjectManagement.Models as well.
// using HumanitarianProjectManagement.Models;

namespace HumanitarianProjectManagement.Models
{
    public class DetailedBudgetLine
    {
        [Key]
        public Guid DetailedBudgetLineID { get; set; } // Changed from int to Guid

        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        // Link to BudgetSubCategory (New)
        public Guid BudgetSubCategoryID { get; set; }
        [ForeignKey("BudgetSubCategoryID")]
        public virtual BudgetSubCategory BudgetSubCategory { get; set; }

        [Required]
        public BudgetCategoriesEnum Category { get; set; } // Using the new enum

        public string Code { get; set; } // e.g., "G.1", "A.3" - will become "G.1.1" etc.

        [StringLength(1500)]
        public string Description { get; set; } // Remarks (max 1500 characters)

        [StringLength(50)]
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Duration { get; set; } // e.g., number of months, days, etc.
        public decimal PercentageChargedToCBPF { get; set; } // Store as 0-100
        public decimal TotalCost { get; set; } // Calculated: Quantity * UnitCost * Duration * (%ChargedToCBPF / 100)

        public BindingList<ItemizedBudgetDetail> ItemizedDetails { get; set; }

        public DetailedBudgetLine()
        {
            DetailedBudgetLineID = Guid.NewGuid();
            Code = string.Empty;
            ItemizedDetails = new BindingList<ItemizedBudgetDetail>();
            Description = string.Empty;
            Unit = string.Empty;
            // Initialize other numeric values to default or sensible values if necessary
            Quantity = 0;
            UnitCost = 0;
            Duration = 1; // Default duration to 1 to avoid TotalCost being zero if not set
            PercentageChargedToCBPF = 100; // Default to 100%
            TotalCost = 0;
            // BudgetSubCategoryID will be set when the line is added to a subcategory
        }
    }
}

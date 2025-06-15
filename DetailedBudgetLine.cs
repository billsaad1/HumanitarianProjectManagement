using System;
using System.Collections.Generic; // Required for ICollection and HashSet
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HumanitarianProjectManagement; // Added for BudgetSubCategory

// Ensure this using directive is present if BudgetCategoriesEnum is in a different namespace,
// but given the plan, it should be in HumanitarianProjectManagement.Models as well.
// using HumanitarianProjectManagement.Models;

namespace HumanitarianProjectManagement.Models
{
    public class DetailedBudgetLine
    {
        [Key]
        public Guid DetailedBudgetLineID { get; set; }

        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        public Guid BudgetSubCategoryID { get; set; }
        [ForeignKey("BudgetSubCategoryID")]
        public virtual BudgetSubCategory BudgetSubCategory { get; set; }

        [Required]
        public BudgetCategoriesEnum Category { get; set; }

        public string Code { get; set; }

        [StringLength(255)]
        public string ItemName { get; set; }

        [StringLength(1500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Duration { get; set; }
        public decimal PercentageChargedToCBPF { get; set; }
        public decimal TotalCost { get; set; }

        // Changed from BindingList<ItemizedBudgetDetail> to virtual ICollection<ItemizedBudgetDetail>
        public virtual ICollection<ItemizedBudgetDetail> ItemizedDetails { get; set; }

        public DetailedBudgetLine()
        {
            DetailedBudgetLineID = Guid.NewGuid();
            Code = string.Empty;
            ItemName = string.Empty;
            Description = string.Empty;
            Unit = string.Empty;
            Quantity = 0;
            UnitCost = 0;
            Duration = 1;
            PercentageChargedToCBPF = 100;
            TotalCost = 0;
            // Initialized with HashSet<ItemizedBudgetDetail>
            ItemizedDetails = new HashSet<ItemizedBudgetDetail>();
        }
    }
}
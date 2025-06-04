using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models
{
    public enum BudgetCategory // Enum for budget categories
    {
        [Description("A. Staff and Other Personnel Costs (Salaries)")]
        StaffAndPersonnel,
        [Description("B. Supplies, Commodities, Materials")]
        SuppliesCommoditiesMaterials,
        [Description("C. Equipment")]
        Equipment,
        [Description("D. Contractual Services")]
        ContractualServices,
        [Description("E. Travel")]
        Travel,
        [Description("F. Transfers and Grants to Counterparts")]
        TransfersAndGrants,
        [Description("G. General Operating and Other Direct Costs")]
        GeneralOperatingAndOtherDirectCosts
    }

    [Table("DetailedBudgetLines")]
    public class DetailedBudgetLine
    {
        [Key]
        public int BudgetLineID { get; set; }

        [Required]
        public int ProjectID { get; set; }
        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        [Required]
        public BudgetCategory Category { get; set; }

        [Required(ErrorMessage = "Budget line description/remarks are required.")]
        [StringLength(1500, ErrorMessage = "Remarks cannot exceed 1500 characters.")]
        [DisplayName("Description/Remarks")]
        public string Remarks { get; set; }

        [StringLength(50)]
        [DisplayName("Unit")]
        public string Unit { get; set; } // e.g., person-month, item, trip

        [Required]
        [DefaultValue(1)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayName("Unit Cost")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than 0.")]
        public decimal UnitCost { get; set; }

        [Required]
        [DefaultValue(1)]
        [DisplayName("Duration (e.g., months, days)")] // Consider if this should be numeric (e.g. number of months) or textual
        public string Duration { get; set; } // Or make it decimal if it's always numeric like months

        [Column(TypeName = "decimal(5, 2)")] // Percentage, e.g., 100.00 or 50.50
        [DisplayName("% Charged to CBPF")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100.")]
        public decimal PercentageChargedToCBPF { get; set; }

        [NotMapped] // Calculated on the fly, or could be stored if preferred
        [DisplayName("Total Cost")]
        public decimal TotalCost
        {
            get { return Quantity * UnitCost; } // This is a basic calculation. May need to factor in Duration or % Charged depending on exact rules.
                                                // The issue implies TotalCost is a direct field to be calculated.
        }

        // Optional: If duration is numeric and part of total cost calculation
        // public decimal? DurationNumeric { get; set; }
        // public decimal TotalCostCalculated => (Quantity * UnitCost * (DurationNumeric ?? 1) * (PercentageChargedToCBPF / 100));


        public DetailedBudgetLine()
        {
            Quantity = 1;
            UnitCost = 0.01m;
            Duration = "1"; // Default duration, e.g., "1 month" or just 1 if numeric
            PercentageChargedToCBPF = 100m; // Default to 100%
        }
    }
}

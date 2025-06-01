using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models
{
    [Table("DetailedBudgetLines")]
    public class DetailedBudgetLine
    {
        [Key]
        public int DetailedBudgetLineID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [Required]
        public BudgetCategory BudgetCategory { get; set; }

        [StringLength(1500)]
        public string Description { get; set; }

        [StringLength(100)] // Assuming a reasonable length for unit
        public string Unit { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitCost { get; set; }

        public int Duration { get; set; } // e.g., number of months, days

        [Column(TypeName = "decimal(5, 2)")] // Assuming PercentChargedToCBPF is like 0.75 for 75%
        public decimal PercentChargedToCBPF { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; } // May be auto-calculated in UI/Service layer

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        public DetailedBudgetLine()
        {
            // Default values if necessary
            Quantity = 0;
            UnitCost = 0m;
            Duration = 0;
            PercentChargedToCBPF = 1.0m; // Default to 100% charged unless specified
            TotalCost = 0m;
        }
    }
}

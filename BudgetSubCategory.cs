using System;
using System.ComponentModel;
using HumanitarianProjectManagement.Models; // Added for Project, DetailedBudgetLine, BudgetCategoriesEnum

namespace HumanitarianProjectManagement
{
    public class BudgetSubCategory
    {
        public Guid BudgetSubCategoryID { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public BudgetCategoriesEnum MainCategory { get; set; }
        public string SubCategoryCodeSuffix { get; set; }
        public string SubCategoryName { get; set; }
        public BindingList<DetailedBudgetLine> DetailedBudgetLines { get; set; }

        public BudgetSubCategory()
        {
            BudgetSubCategoryID = Guid.NewGuid();
            DetailedBudgetLines = new BindingList<DetailedBudgetLine>();
            SubCategoryCodeSuffix = string.Empty;
            SubCategoryName = string.Empty;
        }
    }

    // Assuming BudgetCategoriesEnum exists in this namespace or is imported.
    // If not, it might need to be defined or its namespace imported.
    // For now, I'll assume it's accessible.
    // public enum BudgetCategoriesEnum { /* ... values ... */ } 
    // public class DetailedBudgetLine { /* ... assuming exists ... */ }
    // public class Project { /* ... assuming exists ... */ }
}

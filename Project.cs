using System;
using System.Collections.Generic; // Ensure this is present
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Project name is required.")]
        [StringLength(255)]
        [DisplayName("Project Name")]
        public string ProjectName { get; set; }

        [StringLength(50)]
        [DisplayName("Project Code")]
        public string ProjectCode { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string Location { get; set; }

        [DisplayName("Overall Objective")]
        public string OverallObjective { get; set; } // NVARCHAR(MAX)

        [StringLength(100)]
        public string Status { get; set; }

        [StringLength(255)]
        public string Donor { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayName("Total Budget")]
        public decimal? TotalBudget { get; set; } // This is the overall budget from main form

        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }

        [DisplayName("Updated At")]
        public DateTime UpdatedAt { get; set; } // Matched to user's provided model for this subtask

        public int? SectionID { get; set; } // Foreign key for Section
        [ForeignKey("SectionID")]
        public virtual Section Section { get; set; }

        public int? ManagerUserID { get; set; } // Foreign key for User (Project Manager)
        [ForeignKey("ManagerUserID")]
        public virtual User ManagerUser { get; set; }

        // LogFrame Components
        public virtual ICollection<Outcome> Outcomes { get; set; }

        // NEW: Budget Components
        public virtual ICollection<DetailedBudgetLine> DetailedBudgetLines { get; set; }


        public Project()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow; // Matched to user's provided model for this subtask
            Outcomes = new HashSet<Outcome>();
            DetailedBudgetLines = new HashSet<DetailedBudgetLine>(); // Initialize new collection
            // Note: Other collections like BeneficiaryLists, ProjectIndicators etc. were in the read file but not in the user's target for this subtask.
            // For this operation, I am strictly adhering to the user-provided final structure for Project.cs.
            // If those other collections are needed, they should be in the target state.
        }
    }
}

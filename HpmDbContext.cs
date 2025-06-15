using HumanitarianProjectManagement.Models;
using Microsoft.EntityFrameworkCore; // Required for DbContext and ModelBuilder

namespace HumanitarianProjectManagement.DataAccessLayer
{
    public class HpmDbContext : DbContext
    {
        // Common DbSets - Add any other entities you need EF to manage
        public DbSet<Project> Projects { get; set; }
        public DbSet<Outcome> Outcomes { get; set; }
        public DbSet<Output> Outputs { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ProjectIndicator> ProjectIndicators { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; } // Added for Role management
        public DbSet<UserRole> UserRoles { get; set; } // Added for UserRole management
        public DbSet<Section> Sections { get; set; }
        public DbSet<DetailedBudgetLine> DetailedBudgetLines { get; set; }
        public DbSet<BudgetSubCategory> BudgetSubCategories { get; set; } // Added
        public DbSet<ItemizedBudgetDetail> ItemizedBudgetDetails { get; set; } // Added

        // Add other DbSets as necessary based on your project's models
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<BeneficiaryList> BeneficiaryLists { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<ProjectReport> ProjectReports { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FollowUpVisit> FollowUpVisits { get; set; }

        // Constructor for dependency injection (if you set it up that way)
        public HpmDbContext(DbContextOptions<HpmDbContext> options) : base(options)
        {
        }

        // Parameterless constructor (useful for some design-time tools or if primarily using OnConfiguring)
        public HpmDbContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HumanitarianProjectsDB;Trusted_Connection=True;MultipleActiveResultSets=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Table Name Mappings (Singular names assumed for most custom entities) ---
            // Adjust "ToTable(\"ActualName\")" if your DB table names are different.
            modelBuilder.Entity<Project>().ToTable("Projects");
            modelBuilder.Entity<Outcome>().ToTable("Outcome");
            modelBuilder.Entity<Output>().ToTable("Output");
            modelBuilder.Entity<Activity>().ToTable("Activity");
            modelBuilder.Entity<ProjectIndicator>().ToTable("ProjectIndicator");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<Section>().ToTable("Sections");
            modelBuilder.Entity<DetailedBudgetLine>().ToTable("DetailedBudgetLine");
            modelBuilder.Entity<BudgetSubCategory>().ToTable("BudgetSubCategory");
            modelBuilder.Entity<ItemizedBudgetDetail>().ToTable("ItemizedBudgetDetail");

            modelBuilder.Entity<Beneficiary>().ToTable("Beneficiary");
            modelBuilder.Entity<BeneficiaryList>().ToTable("BeneficiaryList");
            modelBuilder.Entity<PurchaseOrder>().ToTable("PurchaseOrder");
            modelBuilder.Entity<Purchase>().ToTable("Purchase");
            modelBuilder.Entity<StockItem>().ToTable("StockItem");
            modelBuilder.Entity<StockTransaction>().ToTable("StockTransaction");
            modelBuilder.Entity<ProjectReport>().ToTable("ProjectReport");
            modelBuilder.Entity<Feedback>().ToTable("Feedback");
            modelBuilder.Entity<FollowUpVisit>().ToTable("FollowUpVisit");

            // --- Primary Key Configurations (EF Core often infers these if named 'Id' or 'ClassNameId') ---
            modelBuilder.Entity<Project>().HasKey(p => p.ProjectID);
            modelBuilder.Entity<Outcome>().HasKey(o => o.OutcomeID);
            modelBuilder.Entity<Output>().HasKey(op => op.OutputID);
            modelBuilder.Entity<Activity>().HasKey(a => a.ActivityID);
            modelBuilder.Entity<ProjectIndicator>().HasKey(pi => pi.ProjectIndicatorID);
            modelBuilder.Entity<User>().HasKey(u => u.UserID);
            modelBuilder.Entity<Role>().HasKey(r => r.RoleID);
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserID, ur.RoleID });
            modelBuilder.Entity<Section>().HasKey(s => s.SectionID);
            modelBuilder.Entity<DetailedBudgetLine>().HasKey(dbl => dbl.DetailedBudgetLineID);
            modelBuilder.Entity<BudgetSubCategory>().HasKey(bsc => bsc.BudgetSubCategoryID);
            modelBuilder.Entity<ItemizedBudgetDetail>().HasKey(ibd => ibd.ItemizedBudgetDetailID);

            // --- DetailedBudgetLine Configuration ---
            modelBuilder.Entity<DetailedBudgetLine>(entity =>
            {
                entity.HasOne(d => d.Project)
                      .WithMany(p => p.DetailedBudgetLines)
                      .HasForeignKey(d => d.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.BudgetSubCategory)
                      .WithMany(bsc => bsc.DetailedBudgetLines)
                      .HasForeignKey(d => d.BudgetSubCategoryID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
                entity.Property(e => e.Duration).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.PercentageChargedToCBPF).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Description).HasMaxLength(1500);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.ItemName).HasMaxLength(255);
            });

            // --- BudgetSubCategory Configuration ---
            modelBuilder.Entity<BudgetSubCategory>(entity =>
            {
                entity.HasOne(bsc => bsc.Project)
                      .WithMany(p => p.BudgetSubCategories)
                      .HasForeignKey(bsc => bsc.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- ItemizedBudgetDetail Configuration ---
            modelBuilder.Entity<ItemizedBudgetDetail>(entity =>
            {
                entity.HasOne(ibd => ibd.ParentBudgetLine)  // No explicit type for ibd
                      .WithMany(dbl => ((DetailedBudgetLine)dbl).ItemizedDetails)
                      .HasForeignKey(ibd => ibd.ParentBudgetLineID) // No explicit type for ibd
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
                // Any other configurations for ItemizedBudgetDetail...
            });
            // --- LogFrame Component Relationships ---
            modelBuilder.Entity<Outcome>()
                .HasOne(o => o.Project)
                .WithMany(p => p.Outcomes)
                .HasForeignKey(o => o.ProjectID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Outcome>()
                .HasMany(o => o.Outputs)
                .WithOne(op => op.Outcome)
                .HasForeignKey(op => op.OutcomeID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Output>()
                .HasMany(op => op.ProjectIndicators)
                .WithOne(pi => pi.Output)
                .HasForeignKey(pi => pi.OutputID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Output>()
                .HasMany(op => op.Activities)
                .WithOne(a => a.Output)
                .HasForeignKey(a => a.OutputID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- UserRole Many-to-Many Configuration ---
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID);
        }
    }
}

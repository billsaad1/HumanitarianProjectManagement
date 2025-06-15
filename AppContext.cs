using HumanitarianProjectManagement.Models;
// using Microsoft.EntityFrameworkCore; // Required for DbContext and ModelBuilder

namespace HumanitarianProjectManagement.DataAccessLayer // As per instruction
{
    // public class AppContext : DbContext
    public class HpmDbContext // Or make it static if it had static members before, or just a plain class
    {
        // Existing DbSets (guessed from model classes and common practice)
        // public DbSet<Project> Projects { get; set; }
        // public DbSet<Outcome> Outcomes { get; set; }
        // public DbSet<Output> Outputs { get; set; }
        // public DbSet<Activity> Activities { get; set; } // From previous subtask context
        // public DbSet<ProjectIndicator> ProjectIndicators { get; set; }
        // public DbSet<User> Users { get; set; } // Assuming User model exists and is part of context
        // public DbSet<Section> Sections { get; set; } // Assuming Section model exists
        // Add other DbSets as necessary based on your project's models
        // For example:
        // public DbSet<Beneficiary> Beneficiaries { get; set; }
        // public DbSet<BeneficiaryList> BeneficiaryLists { get; set; }
        // ... etc.


        // New DbSet for DetailedBudgetLine
        // public DbSet<DetailedBudgetLine> DetailedBudgetLines { get; set; }

        // public AppContext(DbContextOptions<AppContext> options) : base(options)
        // {
        // }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     if (!optionsBuilder.IsConfigured)
        //     {
        //         // Fallback connection string, ideally configured elsewhere (e.g., Startup.cs or appsettings.json)
        //         // Using the connection string from DatabaseHelper as a placeholder
        //         optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HumanitarianProjectsDB;Trusted_Connection=True;MultipleActiveResultSets=true;");
        //     }
        // }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder); // Recommended to call base method

        //     // Configuration for DetailedBudgetLine
        //     modelBuilder.Entity<DetailedBudgetLine>(entity =>
        //     {
        //         entity.HasOne(d => d.Project)
        //               .WithMany(p => p.DetailedBudgetLines)
        //               .HasForeignKey(d => d.ProjectId)
        //               .OnDelete(DeleteBehavior.Cascade); // Ensures budget lines are deleted if project is deleted

        //         entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 2)");
        //         entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
        //         entity.Property(e => e.Duration).HasColumnType("decimal(18, 2)");
        //         entity.Property(e => e.PercentageChargedToCBPF).HasColumnType("decimal(5, 2)"); // e.g., 75.50 for 75.50%
        //         entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 2)");
        //         entity.Property(e => e.Description).HasMaxLength(1500);
        //         entity.Property(e => e.Unit).HasMaxLength(50);
        //     });

        //     // Example: If ProjectID is the name of the PK in Project model
        //     modelBuilder.Entity<Project>()
        //         .HasKey(p => p.ProjectID);

        //     modelBuilder.Entity<Outcome>()
        //         .HasKey(o => o.OutcomeID);
        //     modelBuilder.Entity<Output>()
        //         .HasKey(op => op.OutputID);
        //     modelBuilder.Entity<Activity>()
        //         .HasKey(a => a.ActivityID);
        //     modelBuilder.Entity<ProjectIndicator>()
        //         .HasKey(pi => pi.ProjectIndicatorID);

        //     // Add other entity configurations here if needed
        //     // For example, configuring relationships for Outcome, Output, Activity, ProjectIndicator
        //     modelBuilder.Entity<Outcome>()
        //         .HasMany(o => o.Outputs)
        //         .WithOne(op => op.Outcome)
        //         .HasForeignKey(op => op.OutcomeID)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     modelBuilder.Entity<Output>()
        //         .HasMany(op => op.ProjectIndicators)
        //         .WithOne(pi => pi.Output)
        //         .HasForeignKey(pi => pi.OutputID)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     modelBuilder.Entity<Output>()
        //         .HasMany(op => op.Activities)
        //         .WithOne(a => a.Output)
        //         .HasForeignKey(a => a.OutputID)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     // Ensure Project relation for Outcome is configured if not done by convention
        //      modelBuilder.Entity<Outcome>()
        //         .HasOne(o => o.Project)
        //         .WithMany(p => p.Outcomes)
        //         .HasForeignKey(o => o.ProjectID)
        //         .OnDelete(DeleteBehavior.ClientSetNull); // Or Cascade if appropriate

        // }
    }
}

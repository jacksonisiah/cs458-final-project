using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinalProject.MVC.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<EventLog> EventLogs { get; set; } = null!;
    public DbSet<Bid> Bids { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<BidStatus>();
        modelBuilder.HasPostgresEnum<ProjectStatus>();

        modelBuilder
            .Entity<Bid>()
            .HasOne(b => b.Project)
            .WithMany(p => p.Bids)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Bid>()
            .HasOne(b => b.Bidder)
            .WithMany()
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<Project>()
            .HasOne(p => p.Submitter)
            .WithMany()
            .HasForeignKey(p => p.SubmitterId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Bid>().Property(b => b.BidStatus).HasColumnType("bid_status");
        modelBuilder
            .Entity<Project>()
            .Property(p => p.ProjectStatus)
            .HasColumnType("project_status");

        modelBuilder
            .Entity<EventLog>()
            .HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<EventLog>()
            .HasOne(e => e.Bid)
            .WithMany()
            .HasForeignKey(e => e.BidId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

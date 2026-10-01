using DonationTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DonationTracker.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Supporter> Supporters => Set<Supporter>();

    public DbSet<Donation> Donations => Set<Donation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Covering index for "one supporter's donations, newest first".
        // It was first created and measured by hand; see sql/PLAN-NOTES.md.
        modelBuilder.Entity<Donation>()
            .HasIndex(d => new { d.SupporterId, d.DonatedOn })
            .IsDescending(false, true)
            .IncludeProperties(d => d.Amount);
    }
}

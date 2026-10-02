using DonationTracker.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DonationTracker.Web.Data;

// IdentityDbContext adds the tables ASP.NET Core Identity needs (users, roles
// and so on) alongside our own.
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Supporter> Supporters => Set<Supporter>();

    public DbSet<Donation> Donations => Set<Donation>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

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

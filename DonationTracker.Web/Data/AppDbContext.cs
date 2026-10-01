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
}

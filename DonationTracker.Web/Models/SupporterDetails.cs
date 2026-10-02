namespace DonationTracker.Web.Models;

// Everything the supporter details page shows.
public class SupporterDetails
{
    public Supporter Supporter { get; set; } = new();

    public List<Donation> Donations { get; set; } = new();

    public decimal Total { get; set; }
}

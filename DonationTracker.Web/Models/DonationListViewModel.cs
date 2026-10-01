namespace DonationTracker.Web.Models;

public class DonationListViewModel
{
    public List<Donation> Donations { get; set; } = new();

    public decimal CampaignTotal { get; set; }
}

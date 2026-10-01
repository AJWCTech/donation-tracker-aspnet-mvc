namespace DonationTracker.Web.Models;

public class DonationListViewModel
{
    public DonationPage Page { get; set; } = new();

    public decimal CampaignTotal { get; set; }
}

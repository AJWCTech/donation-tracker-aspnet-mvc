using DonationTracker.Web.Models;

namespace DonationTracker.Web.Services;

public interface IDonationService
{
    Task<List<Supporter>> GetSupportersAsync();

    Task<Supporter?> GetSupporterAsync(int id);

    Task CreateSupporterAsync(Supporter supporter);

    // Returns false when no supporter with that Id exists.
    Task<bool> UpdateSupporterAsync(Supporter supporter);

    // Returns the most recent donations only, newest first.
    Task<List<Donation>> GetRecentDonationsAsync();

    Task<Donation?> GetDonationAsync(int id);

    Task<decimal> GetCampaignTotalAsync();

    Task CreateDonationAsync(Donation donation);

    // Returns false when no donation with that Id exists.
    Task<bool> UpdateDonationAsync(Donation donation);
}

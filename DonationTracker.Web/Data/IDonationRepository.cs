using DonationTracker.Web.Models;

namespace DonationTracker.Web.Data;

public interface IDonationRepository
{
    Task<List<Supporter>> GetSupportersAsync();

    Task<Supporter?> GetSupporterByIdAsync(int id);

    Task AddSupporterAsync(Supporter supporter);

    Task UpdateSupporterAsync(Supporter supporter);

    Task<List<Donation>> GetDonationsAsync();

    Task<List<Donation>> GetRecentDonationsAsync(int count);

    Task<Donation?> GetDonationByIdAsync(int id);

    Task AddDonationAsync(Donation donation);

    Task UpdateDonationAsync(Donation donation);
}

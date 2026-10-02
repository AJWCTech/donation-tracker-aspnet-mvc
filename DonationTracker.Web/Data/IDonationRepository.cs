using DonationTracker.Web.Models;

namespace DonationTracker.Web.Data;

public interface IDonationRepository
{
    Task<List<Supporter>> GetSupportersAsync();

    Task<Supporter?> GetSupporterByIdAsync(int id);

    Task AddSupporterAsync(Supporter supporter);

    Task UpdateSupporterAsync(Supporter supporter);

    Task<decimal> GetDonationTotalAsync();

    Task<int> CountDonationsAsync();

    // pageNumber starts at 1. Newest donations come first.
    Task<List<Donation>> GetDonationsPageAsync(int pageNumber, int pageSize);

    // One supporter's donations, newest first, read through a stored procedure.
    Task<List<Donation>> GetDonationsBySupporterAsync(int supporterId);

    Task<Donation?> GetDonationByIdAsync(int id);

    Task AddDonationAsync(Donation donation);

    Task UpdateDonationAsync(Donation donation);
}

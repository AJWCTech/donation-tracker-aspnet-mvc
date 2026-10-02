using DonationTracker.Web.Models;

namespace DonationTracker.Web.Services;

public interface IDonationService
{
    Task<List<Supporter>> GetSupportersAsync();

    Task<Supporter?> GetSupporterAsync(int id);

    // The supporter with their donations and total. Null when the supporter does not exist.
    Task<SupporterDetails?> GetSupporterDetailsAsync(int id);

    // changedBy is the signed-in user, recorded in the audit trail.
    Task CreateSupporterAsync(Supporter supporter, string changedBy);

    // Returns false when no supporter with that Id exists.
    Task<bool> UpdateSupporterAsync(Supporter supporter, string changedBy);

    // Returns one page of donations, newest first. Page numbers start at 1.
    Task<DonationPage> GetDonationPageAsync(int pageNumber, int pageSize);

    // The same page, mapped to the shape the API returns.
    Task<DonationPageDto> GetDonationPageDtoAsync(int pageNumber, int pageSize);

    Task<Donation?> GetDonationAsync(int id);

    Task<decimal> GetCampaignTotalAsync();

    Task CreateDonationAsync(Donation donation, string changedBy);

    // Returns false when no donation with that Id exists.
    Task<bool> UpdateDonationAsync(Donation donation, string changedBy);

    // The newest audit entries first.
    Task<List<AuditEntry>> GetRecentAuditEntriesAsync();
}

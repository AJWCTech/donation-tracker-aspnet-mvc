using DonationTracker.Web.Models;

namespace DonationTracker.Web.Data;

public interface IDonationRepository
{
    Task<List<Supporter>> GetSupportersAsync();

    Task<Supporter?> GetSupporterByIdAsync(int id);

    // Every add and update saves its audit entry in the same transaction.
    Task AddSupporterAsync(Supporter supporter, AuditEntry auditEntry);

    Task UpdateSupporterAsync(Supporter supporter, AuditEntry auditEntry);

    Task<decimal> GetDonationTotalAsync();

    Task<int> CountDonationsAsync();

    // pageNumber starts at 1. Newest donations come first.
    Task<List<Donation>> GetDonationsPageAsync(int pageNumber, int pageSize);

    // One supporter's donations, newest first, read through a stored procedure.
    Task<List<Donation>> GetDonationsBySupporterAsync(int supporterId);

    Task<Donation?> GetDonationByIdAsync(int id);

    Task AddDonationAsync(Donation donation, AuditEntry auditEntry);

    Task UpdateDonationAsync(Donation donation, AuditEntry auditEntry);

    // The newest audit entries first.
    Task<List<AuditEntry>> GetRecentAuditEntriesAsync(int count);
}

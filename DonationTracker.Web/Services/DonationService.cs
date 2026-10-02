using DonationTracker.Web.Data;
using DonationTracker.Web.Models;

namespace DonationTracker.Web.Services;

public class DonationService : IDonationService
{
    private const int MaxPageSize = 100;
    private const int RecentAuditEntryCount = 100;

    private readonly IDonationRepository _repository;

    public DonationService(IDonationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Supporter>> GetSupportersAsync()
    {
        return await _repository.GetSupportersAsync();
    }

    public async Task<Supporter?> GetSupporterAsync(int id)
    {
        return await _repository.GetSupporterByIdAsync(id);
    }

    public async Task<SupporterDetails?> GetSupporterDetailsAsync(int id)
    {
        Supporter? supporter = await _repository.GetSupporterByIdAsync(id);
        if (supporter == null)
        {
            return null;
        }

        List<Donation> donations = await _repository.GetDonationsBySupporterAsync(id);

        return new SupporterDetails
        {
            Supporter = supporter,
            Donations = donations,
            Total = donations.Sum(d => d.Amount)
        };
    }

    public async Task CreateSupporterAsync(Supporter supporter, string changedBy)
    {
        supporter.CreatedOn = DateTime.UtcNow;

        string summary = $"Full name '{supporter.FullName}', email '{supporter.Email}'";
        AuditEntry auditEntry = BuildAuditEntry("Created", "Supporter", supporter.Id, changedBy, summary);

        await _repository.AddSupporterAsync(supporter, auditEntry);
    }

    public async Task<bool> UpdateSupporterAsync(Supporter supporter, string changedBy)
    {
        Supporter? existing = await _repository.GetSupporterByIdAsync(supporter.Id);
        if (existing == null)
        {
            return false;
        }

        // Describe what is changing before the old values are overwritten.
        List<string> changes = new List<string>();
        if (existing.FullName != supporter.FullName)
        {
            changes.Add($"Full name '{existing.FullName}' to '{supporter.FullName}'");
        }

        if (existing.Email != supporter.Email)
        {
            changes.Add($"Email '{existing.Email}' to '{supporter.Email}'");
        }

        AuditEntry auditEntry = BuildAuditEntry("Updated", "Supporter", existing.Id, changedBy, DescribeChanges(changes));

        // Copy only the editable fields so CreatedOn keeps its original value.
        existing.FullName = supporter.FullName;
        existing.Email = supporter.Email;
        await _repository.UpdateSupporterAsync(existing, auditEntry);
        return true;
    }

    public async Task<DonationPage> GetDonationPageAsync(int pageNumber, int pageSize)
    {
        // Keep the page size within sensible limits.
        if (pageSize < 1)
        {
            pageSize = 1;
        }

        if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        int totalCount = await _repository.CountDonationsAsync();

        // Round up, so 51 rows at 25 per page is 3 pages. Always at least 1.
        int totalPages = (totalCount + pageSize - 1) / pageSize;
        if (totalPages < 1)
        {
            totalPages = 1;
        }

        // A page number outside the range is moved to the nearest valid page.
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        List<Donation> donations = await _repository.GetDonationsPageAsync(pageNumber, pageSize);

        return new DonationPage
        {
            Donations = donations,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<DonationPageDto> GetDonationPageDtoAsync(int pageNumber, int pageSize)
    {
        DonationPage page = await GetDonationPageAsync(pageNumber, pageSize);

        List<DonationDto> items = new List<DonationDto>();
        foreach (Donation donation in page.Donations)
        {
            DonationDto dto = new DonationDto
            {
                Id = donation.Id,
                SupporterName = donation.Supporter?.FullName ?? string.Empty,
                Amount = donation.Amount,
                DonatedOn = donation.DonatedOn
            };
            items.Add(dto);
        }

        return new DonationPageDto
        {
            Items = items,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages
        };
    }

    public async Task<Donation?> GetDonationAsync(int id)
    {
        return await _repository.GetDonationByIdAsync(id);
    }

    public async Task<decimal> GetCampaignTotalAsync()
    {
        return await _repository.GetDonationTotalAsync();
    }

    public async Task CreateDonationAsync(Donation donation, string changedBy)
    {
        await CheckDonationRulesAsync(donation);

        string summary = $"Supporter {donation.SupporterId}, amount {donation.Amount:F2}, donated on {donation.DonatedOn:yyyy-MM-dd}";
        AuditEntry auditEntry = BuildAuditEntry("Created", "Donation", donation.Id, changedBy, summary);

        await _repository.AddDonationAsync(donation, auditEntry);
    }

    public async Task<bool> UpdateDonationAsync(Donation donation, string changedBy)
    {
        Donation? existing = await _repository.GetDonationByIdAsync(donation.Id);
        if (existing == null)
        {
            return false;
        }

        await CheckDonationRulesAsync(donation);

        List<string> changes = new List<string>();
        if (existing.SupporterId != donation.SupporterId)
        {
            changes.Add($"Supporter {existing.SupporterId} to {donation.SupporterId}");
        }

        if (existing.Amount != donation.Amount)
        {
            changes.Add($"Amount {existing.Amount:F2} to {donation.Amount:F2}");
        }

        if (existing.DonatedOn != donation.DonatedOn)
        {
            changes.Add($"Donated on {existing.DonatedOn:yyyy-MM-dd} to {donation.DonatedOn:yyyy-MM-dd}");
        }

        AuditEntry auditEntry = BuildAuditEntry("Updated", "Donation", existing.Id, changedBy, DescribeChanges(changes));

        existing.SupporterId = donation.SupporterId;
        existing.Amount = donation.Amount;
        existing.DonatedOn = donation.DonatedOn;
        await _repository.UpdateDonationAsync(existing, auditEntry);
        return true;
    }

    public async Task<List<AuditEntry>> GetRecentAuditEntriesAsync()
    {
        return await _repository.GetRecentAuditEntriesAsync(RecentAuditEntryCount);
    }

    private static AuditEntry BuildAuditEntry(string action, string entityName, int entityId, string changedBy, string summary)
    {
        return new AuditEntry
        {
            ChangedOn = DateTime.UtcNow,
            ChangedBy = changedBy,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Summary = summary
        };
    }

    private static string DescribeChanges(List<string> changes)
    {
        if (changes.Count == 0)
        {
            return "Saved with no changes";
        }

        return string.Join("; ", changes);
    }

    private async Task CheckDonationRulesAsync(Donation donation)
    {
        if (donation.DonatedOn.Date > DateTime.Today)
        {
            throw new BusinessRuleException("The donation date cannot be in the future.");
        }

        Supporter? supporter = await _repository.GetSupporterByIdAsync(donation.SupporterId);
        if (supporter == null)
        {
            throw new BusinessRuleException("The selected supporter does not exist.");
        }
    }
}

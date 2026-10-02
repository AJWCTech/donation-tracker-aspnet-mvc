using DonationTracker.Web.Data;
using DonationTracker.Web.Models;

namespace DonationTracker.Web.Services;

public class DonationService : IDonationService
{
    private const int MaxPageSize = 100;

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

    public async Task CreateSupporterAsync(Supporter supporter)
    {
        supporter.CreatedOn = DateTime.UtcNow;
        await _repository.AddSupporterAsync(supporter);
    }

    public async Task<bool> UpdateSupporterAsync(Supporter supporter)
    {
        Supporter? existing = await _repository.GetSupporterByIdAsync(supporter.Id);
        if (existing == null)
        {
            return false;
        }

        // Copy only the editable fields so CreatedOn keeps its original value.
        existing.FullName = supporter.FullName;
        existing.Email = supporter.Email;
        await _repository.UpdateSupporterAsync(existing);
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

    public async Task CreateDonationAsync(Donation donation)
    {
        await CheckDonationRulesAsync(donation);
        await _repository.AddDonationAsync(donation);
    }

    public async Task<bool> UpdateDonationAsync(Donation donation)
    {
        Donation? existing = await _repository.GetDonationByIdAsync(donation.Id);
        if (existing == null)
        {
            return false;
        }

        await CheckDonationRulesAsync(donation);

        existing.SupporterId = donation.SupporterId;
        existing.Amount = donation.Amount;
        existing.DonatedOn = donation.DonatedOn;
        await _repository.UpdateDonationAsync(existing);
        return true;
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

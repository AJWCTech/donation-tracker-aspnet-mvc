using DonationTracker.Web.Data;
using DonationTracker.Web.Models;

namespace DonationTracker.Web.Services;

public class DonationService : IDonationService
{
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

    public async Task<List<Donation>> GetDonationsAsync()
    {
        return await _repository.GetDonationsAsync();
    }

    public async Task<Donation?> GetDonationAsync(int id)
    {
        return await _repository.GetDonationByIdAsync(id);
    }

    public async Task<decimal> GetCampaignTotalAsync()
    {
        List<Donation> donations = await _repository.GetDonationsAsync();
        return donations.Sum(d => d.Amount);
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

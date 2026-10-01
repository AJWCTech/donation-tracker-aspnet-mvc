using DonationTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DonationTracker.Web.Data;

public class DonationRepository : IDonationRepository
{
    private readonly AppDbContext _context;

    public DonationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Supporter>> GetSupportersAsync()
    {
        return await _context.Supporters
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<Supporter?> GetSupporterByIdAsync(int id)
    {
        return await _context.Supporters.FindAsync(id);
    }

    public async Task AddSupporterAsync(Supporter supporter)
    {
        _context.Supporters.Add(supporter);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSupporterAsync(Supporter supporter)
    {
        _context.Supporters.Update(supporter);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Donation>> GetDonationsAsync()
    {
        return await _context.Donations
            .Include(d => d.Supporter)
            .OrderByDescending(d => d.DonatedOn)
            .ToListAsync();
    }

    public async Task<List<Donation>> GetRecentDonationsAsync(int count)
    {
        return await _context.Donations
            .Include(d => d.Supporter)
            .OrderByDescending(d => d.DonatedOn)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Donation?> GetDonationByIdAsync(int id)
    {
        return await _context.Donations.FindAsync(id);
    }

    public async Task AddDonationAsync(Donation donation)
    {
        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDonationAsync(Donation donation)
    {
        _context.Donations.Update(donation);
        await _context.SaveChangesAsync();
    }
}

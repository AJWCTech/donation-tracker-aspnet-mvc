using DonationTracker.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    public async Task AddSupporterAsync(Supporter supporter, AuditEntry auditEntry)
    {
        // The new row and its audit entry are saved in one transaction, so
        // there is never a change without a record of it.
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

        _context.Supporters.Add(supporter);
        await _context.SaveChangesAsync();

        // The database only gives the new row its Id on that first save.
        auditEntry.EntityId = supporter.Id;
        _context.AuditEntries.Add(auditEntry);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task UpdateSupporterAsync(Supporter supporter, AuditEntry auditEntry)
    {
        // A single SaveChanges call is already one transaction.
        _context.Supporters.Update(supporter);
        _context.AuditEntries.Add(auditEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetDonationTotalAsync()
    {
        // Translated to SELECT SUM(Amount), so the database does the adding up.
        return await _context.Donations.SumAsync(d => d.Amount);
    }

    public async Task<int> CountDonationsAsync()
    {
        return await _context.Donations.CountAsync();
    }

    public async Task<List<Donation>> GetDonationsPageAsync(int pageNumber, int pageSize)
    {
        // Ordering by Id as well keeps rows with the same date in a fixed
        // order, so a row cannot appear on two pages.
        return await _context.Donations
            .Include(d => d.Supporter)
            .OrderByDescending(d => d.DonatedOn)
            .ThenByDescending(d => d.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Donation>> GetDonationsBySupporterAsync(int supporterId)
    {
        // FromSqlInterpolated turns {supporterId} into a SQL parameter. The
        // value is never pasted into the SQL text, so it cannot be used for
        // SQL injection.
        return await _context.Donations
            .FromSqlInterpolated($"EXEC dbo.usp_GetDonationsBySupporter @SupporterId = {supporterId}")
            .ToListAsync();
    }

    public async Task<Donation?> GetDonationByIdAsync(int id)
    {
        return await _context.Donations.FindAsync(id);
    }

    public async Task AddDonationAsync(Donation donation, AuditEntry auditEntry)
    {
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        auditEntry.EntityId = donation.Id;
        _context.AuditEntries.Add(auditEntry);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task UpdateDonationAsync(Donation donation, AuditEntry auditEntry)
    {
        _context.Donations.Update(donation);
        _context.AuditEntries.Add(auditEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditEntry>> GetRecentAuditEntriesAsync(int count)
    {
        return await _context.AuditEntries
            .OrderByDescending(a => a.ChangedOn)
            .ThenByDescending(a => a.Id)
            .Take(count)
            .ToListAsync();
    }
}

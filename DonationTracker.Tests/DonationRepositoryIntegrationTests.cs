using DonationTracker.Web.Data;
using DonationTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DonationTracker.Tests;

// These tests run the real repository against a real SQL Server LocalDB
// database. The database is created from the migrations before the first test
// and dropped after the last one, so the app's own database is never touched.
[TestFixture]
[Category("Integration")]
public class DonationRepositoryIntegrationTests
{
    private const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=DonationTracker_IntegrationTests;Trusted_Connection=True;";

    private AppDbContext _context = null!;
    private DonationRepository _repository = null!;

    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    [OneTimeSetUp]
    public void CreateDatabase()
    {
        using AppDbContext context = CreateContext();
        context.Database.EnsureDeleted();
        context.Database.Migrate();
    }

    [OneTimeTearDown]
    public void DropDatabase()
    {
        using AppDbContext context = CreateContext();
        context.Database.EnsureDeleted();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Every test starts with empty tables and its own context.
        _context = CreateContext();
        await _context.Donations.ExecuteDeleteAsync();
        await _context.Supporters.ExecuteDeleteAsync();
        _repository = new DonationRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private async Task<Supporter> AddSupporterAsync(string fullName)
    {
        Supporter supporter = new Supporter
        {
            FullName = fullName,
            Email = "test@example.com",
            CreatedOn = DateTime.UtcNow
        };
        await _repository.AddSupporterAsync(supporter);
        return supporter;
    }

    private async Task AddDonationAsync(Supporter supporter, decimal amount, DateTime donatedOn)
    {
        Donation donation = new Donation
        {
            SupporterId = supporter.Id,
            Amount = amount,
            DonatedOn = donatedOn
        };
        await _repository.AddDonationAsync(donation);
    }

    [Test]
    public async Task GetDonationTotalAsync_ReturnsZero_WhenThereAreNoDonations()
    {
        decimal total = await _repository.GetDonationTotalAsync();

        Assert.That(total, Is.EqualTo(0m));
    }

    [Test]
    public async Task GetDonationTotalAsync_SumsAllDonationAmounts()
    {
        Supporter supporter = await AddSupporterAsync("Total Tester");
        await AddDonationAsync(supporter, 10.50m, new DateTime(2026, 1, 1));
        await AddDonationAsync(supporter, 20.25m, new DateTime(2026, 1, 2));
        await AddDonationAsync(supporter, 100m, new DateTime(2026, 1, 3));

        decimal total = await _repository.GetDonationTotalAsync();

        Assert.That(total, Is.EqualTo(130.75m));
    }

    [Test]
    public async Task CountDonationsAsync_ReturnsTheNumberOfRows()
    {
        Supporter supporter = await AddSupporterAsync("Count Tester");
        await AddDonationAsync(supporter, 5m, new DateTime(2026, 1, 1));
        await AddDonationAsync(supporter, 5m, new DateTime(2026, 1, 2));

        int count = await _repository.CountDonationsAsync();

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetDonationsPageAsync_ReturnsNewestFirst_OnePageAtATime()
    {
        Supporter supporter = await AddSupporterAsync("Page Tester");
        await AddDonationAsync(supporter, 1m, new DateTime(2026, 1, 1));
        await AddDonationAsync(supporter, 2m, new DateTime(2026, 1, 2));
        await AddDonationAsync(supporter, 3m, new DateTime(2026, 1, 3));

        List<Donation> firstPage = await _repository.GetDonationsPageAsync(1, 2);
        List<Donation> secondPage = await _repository.GetDonationsPageAsync(2, 2);

        Assert.That(firstPage.Select(d => d.Amount), Is.EqualTo(new[] { 3m, 2m }));
        Assert.That(secondPage.Select(d => d.Amount), Is.EqualTo(new[] { 1m }));
        Assert.That(firstPage[0].Supporter!.FullName, Is.EqualTo("Page Tester"));
    }
}

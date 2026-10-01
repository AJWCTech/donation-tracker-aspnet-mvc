using DonationTracker.Web.Data;
using DonationTracker.Web.Models;
using DonationTracker.Web.Services;
using Moq;

namespace DonationTracker.Tests;

[TestFixture]
public class DonationServiceTests
{
    private Mock<IDonationRepository> _repository = null!;
    private DonationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        // A fresh mock for every test, so no test can affect another.
        _repository = new Mock<IDonationRepository>();
        _service = new DonationService(_repository.Object);
    }

    [Test]
    public async Task GetCampaignTotalAsync_SumsAllDonationAmounts()
    {
        List<Donation> donations = new List<Donation>
        {
            new Donation { Amount = 10.50m },
            new Donation { Amount = 20.25m },
            new Donation { Amount = 100m }
        };
        _repository.Setup(r => r.GetDonationsAsync()).ReturnsAsync(donations);

        decimal total = await _service.GetCampaignTotalAsync();

        Assert.That(total, Is.EqualTo(130.75m));
    }

    [Test]
    public async Task GetCampaignTotalAsync_ReturnsZero_WhenThereAreNoDonations()
    {
        _repository.Setup(r => r.GetDonationsAsync()).ReturnsAsync(new List<Donation>());

        decimal total = await _service.GetCampaignTotalAsync();

        Assert.That(total, Is.EqualTo(0m));
    }

    [Test]
    public void CreateDonationAsync_Throws_WhenDateIsInTheFuture()
    {
        _repository.Setup(r => r.GetSupporterByIdAsync(1)).ReturnsAsync(new Supporter { Id = 1 });
        Donation donation = new Donation
        {
            SupporterId = 1,
            Amount = 25m,
            DonatedOn = DateTime.Today.AddDays(1)
        };

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateDonationAsync(donation));
        _repository.Verify(r => r.AddDonationAsync(It.IsAny<Donation>()), Times.Never);
    }

    [Test]
    public void CreateDonationAsync_Throws_WhenSupporterDoesNotExist()
    {
        _repository.Setup(r => r.GetSupporterByIdAsync(99)).ReturnsAsync((Supporter?)null);
        Donation donation = new Donation
        {
            SupporterId = 99,
            Amount = 25m,
            DonatedOn = DateTime.Today
        };

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateDonationAsync(donation));
        _repository.Verify(r => r.AddDonationAsync(It.IsAny<Donation>()), Times.Never);
    }

    [Test]
    public async Task CreateDonationAsync_AddsDonationOnce_WhenDonationIsValid()
    {
        _repository.Setup(r => r.GetSupporterByIdAsync(1)).ReturnsAsync(new Supporter { Id = 1 });
        Donation donation = new Donation
        {
            SupporterId = 1,
            Amount = 25m,
            DonatedOn = DateTime.Today
        };

        await _service.CreateDonationAsync(donation);

        _repository.Verify(r => r.AddDonationAsync(donation), Times.Once);
    }
}

namespace DonationTracker.Web.Models;

// The shape the API returns. It is separate from the Donation entity so the
// JSON contract does not change when the database model does.
public class DonationDto
{
    public int Id { get; set; }

    public string SupporterName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime DonatedOn { get; set; }
}

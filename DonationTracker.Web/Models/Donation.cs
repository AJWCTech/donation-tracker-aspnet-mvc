using Microsoft.EntityFrameworkCore;

namespace DonationTracker.Web.Models;

public class Donation
{
    public int Id { get; set; }

    public int SupporterId { get; set; }

    [Precision(10, 2)]
    public decimal Amount { get; set; }

    public DateTime DonatedOn { get; set; }

    public Supporter? Supporter { get; set; }
}

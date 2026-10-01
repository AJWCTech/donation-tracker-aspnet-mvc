namespace DonationTracker.Web.Models;

// One page of donations plus the numbers needed to draw the paging links.
public class DonationPage
{
    public List<Donation> Donations { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}

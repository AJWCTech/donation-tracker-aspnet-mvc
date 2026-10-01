namespace DonationTracker.Web.Models;

// The shape GET /api/donations returns: one page of items plus paging details.
public class DonationPageDto
{
    public List<DonationDto> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}

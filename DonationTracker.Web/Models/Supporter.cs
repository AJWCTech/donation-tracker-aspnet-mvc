using System.ComponentModel.DataAnnotations;

namespace DonationTracker.Web.Models;

public class Supporter
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public List<Donation> Donations { get; set; } = new();
}

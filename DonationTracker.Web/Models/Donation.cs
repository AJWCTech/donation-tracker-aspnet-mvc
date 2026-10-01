using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DonationTracker.Web.Models;

public class Donation
{
    public int Id { get; set; }

    [Display(Name = "Supporter")]
    public int SupporterId { get; set; }

    [Precision(10, 2)]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Donated on")]
    public DateTime DonatedOn { get; set; }

    public Supporter? Supporter { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace DonationTracker.Web.Models;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // Where to send the user after signing in.
    public string? ReturnUrl { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace DonationTracker.Web.Data;

// Creates the staff account at startup, if one is configured and it does not
// exist yet. The email and password come from configuration (user secrets in
// development), so no password is ever stored in the repository.
public static class StaffSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        string? email = configuration["SeedStaff:Email"];
        string? password = configuration["SeedStaff:Password"];
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return;
        }

        UserManager<IdentityUser> userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        IdentityUser? existing = await userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            return;
        }

        IdentityUser user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        // CreateAsync hashes the password; the plain text is never saved.
        IdentityResult result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            string errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException("Could not create the staff account: " + errors);
        }
    }
}

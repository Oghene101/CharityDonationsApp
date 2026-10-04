using CharityDonationsApp.Domain.Constants;
using CharityDonationsApp.Domain.Entities;
using CharityDonationsApp.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CharityDonationsApp.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var seed = scope.ServiceProvider.GetRequiredService<IOptions<SeedSettings>>().Value;

        // Ensure roles exist
        foreach (var role in Roles.List)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // Seed an admin user
        await SeedAccountAsync(userManager, Roles.Admin, seed.Admin);

        // Seed a user
        await SeedAccountAsync(userManager, Roles.User, seed.User);
    }

    // Accounts are only seeded when credentials are configured and the role has no members yet
    private static async Task SeedAccountAsync(UserManager<User> userManager, string role, SeedAccount account)
    {
        if (string.IsNullOrWhiteSpace(account.Email) || string.IsNullOrWhiteSpace(account.Password)) return;

        var existing = await userManager.GetUsersInRoleAsync(role);
        if (existing.Count != 0) return;

        var user = new User
        {
            FirstName = role,
            LastName = "Test",
            UserName = account.Email,
            Email = account.Email,
            EmailConfirmed = true,
            CreatedBy = "Seeder",
            UpdatedBy = "Seeder",
        };

        var result = await userManager.CreateAsync(user, account.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
        else
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
using Microsoft.AspNetCore.Identity;
using UserManagementApi.Models;

namespace UserManagementApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var roleManager =
            services.GetRequiredService<
                RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        string[] roles =
        {
            "User",
            "Admin"
        };

        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }

        string? adminEmail =
            configuration["SeedAdmin:Email"];

        string? adminPassword =
            configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var admin =
            await userManager.FindByEmailAsync(
                adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Name = "Administrator",
                Age = 30
            };

            IdentityResult result =
                await userManager.CreateAsync(
                    admin,
                    adminPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        result.Errors.Select(
                            e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(
                admin,
                "Admin"))
        {
            await userManager.AddToRoleAsync(
                admin,
                "Admin");
        }
    }
}

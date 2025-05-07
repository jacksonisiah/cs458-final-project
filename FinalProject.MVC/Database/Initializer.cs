using System;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FinalProject.MVC.Data;

public static class Initializer
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "ProjectManager", "Bidder" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Log.Information($"Role '{roleName}' created.");
            }
        }

        var email = "no@catfile.me";
        var name = "jackson";
        var pw = "Password123!";

        var userCount = userManager.Users.Count();
        if (userCount > 0)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = name,
        };
        var result = await userManager.CreateAsync(user, pw);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
            Log.Warning($"Seeded initial admin user");
        }
        else
        {
            Log.Error(
                $"Failed to create initial admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
    }
}

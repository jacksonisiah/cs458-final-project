using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinalProject.MVC;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSerilog();

        var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                o =>
                {
                    o.MapEnum<BidStatus>();
                    o.MapEnum<ProjectStatus>();
                }
            )
        );
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder
            .Services.AddDefaultIdentity<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = true
            )
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        using (var scope = app.Services.CreateScope())
        {
            await Initializer.SeedData(scope.ServiceProvider);
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        app.MapRazorPages().WithStaticAssets();

        await app.RunAsync("http://localhost:5001");
    }
}

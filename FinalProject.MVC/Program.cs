using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using FinalProject.MVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

        builder.Services.AddHttpClient();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder
            .Services.AddDefaultIdentity<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = true
            )
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Corrected registration:
        builder.Services.AddTransient<
            Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>,
            MailerService
        >();
        builder.Services.AddTransient<
            Microsoft.AspNetCore.Identity.UI.Services.IEmailSender,
            MailerService
        >();

        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<EventLogService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "1" });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API"));
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

        await app.RunAsync($"http://0.0.0.0:{app.Configuration["Port"]}");
    }
}

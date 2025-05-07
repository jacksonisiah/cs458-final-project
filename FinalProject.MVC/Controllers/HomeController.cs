using System.Diagnostics;
using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.MVC.Controllers;

[Authorize]
public class HomeController(
    ILogger<HomeController> logger,
    ApplicationDbContext ctx,
    UserManager<ApplicationUser> userManager
) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly ApplicationDbContext _ctx = ctx;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _ctx
                .Projects.Include(p => p.Submitter)
                .Include(p => p.Bids)
                .ThenInclude(b => b.Bidder)
                .OrderByDescending(p => p.Deadline)
                .ToListAsync();

            return View(projects);
        }
        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Help()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}

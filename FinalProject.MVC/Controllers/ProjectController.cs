using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using FinalProject.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.MVC.Controllers;

[Authorize]
public class ProjectController(
    ApplicationDbContext ctx,
    UserManager<ApplicationUser> userManager,
    EventLogService eventLogService,
    MailerService mailerService
) : Controller
{
    private readonly ApplicationDbContext _ctx = ctx;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly EventLogService _eventLogService = eventLogService;
    private readonly MailerService _mailerService = mailerService;

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var project = await _ctx
            .Projects.Include(p => p.Submitter)
            .Include(p => p.Bids)
            .ThenInclude(b => b.Bidder)
            .FirstOrDefaultAsync(m => m.ProjectId == id);

        if (project == null)
            return NotFound();

        var eventLogs = await _ctx
            .EventLogs.Where(e => e.ProjectId == id)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();

        var eventLogDisplays = new List<EventLogViewModel>();
        foreach (var log in eventLogs)
        {
            var user = await _userManager.FindByIdAsync(log.UserId);
            eventLogDisplays.Add(
                new EventLogViewModel
                {
                    EventDate = log.EventDate,
                    UserDisplayName = user?.DisplayName ?? user?.UserName ?? "Unknown User",
                    EventType = log.EventType,
                    EventDescription = log.EventDescription,
                }
            );
        }
        ViewBag.EventLogs = eventLogDisplays;

        return View(project);
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Tags,Deadline,ProjectStatus,Funding")] Project project
    )
    {
        if (ModelState.IsValid)
        {
            project.Deadline = DateTime.SpecifyKind(project.Deadline, DateTimeKind.Utc);
            project.ProjectStatus = ProjectStatus.Submitted;
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("not logged in?");
            }
            var user = await _userManager.GetUserAsync(User);
            project.SubmitterId = userId;
            _ctx.Add(project);
            await _ctx.SaveChangesAsync();
            await _eventLogService.LogEventAsync(
                userId,
                "Project Created",
                $"Project '{project.Title}' was created",
                project.ProjectId
            );
            return RedirectToAction("Index", "Home");
        }
        return View(project);
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var project = await _ctx.Projects.FindAsync(id);
        if (project == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (project.SubmitterId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("ProjectId,Title,Description,Tags,Deadline,ProjectStatus,Funding,SubmitterId")]
            Project project
    )
    {
        if (id != project.ProjectId)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        var existingProject = await _ctx
            .Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == id);
        if (existingProject == null)
            return NotFound();

        if (!User.IsInRole("Admin"))
        {
            if (existingProject.SubmitterId != userId)
                return Forbid();

            project.ProjectStatus = existingProject.ProjectStatus;
            project.SubmitterId = existingProject.SubmitterId;
        }
        else if (
            string.IsNullOrEmpty(project.SubmitterId)
            && !string.IsNullOrEmpty(existingProject.SubmitterId)
        )
        {
            project.SubmitterId = existingProject.SubmitterId;
        }

        if (ModelState.IsValid)
        {
            try
            {
                project.Deadline = DateTime.SpecifyKind(project.Deadline, DateTimeKind.Utc);
                _ctx.Update(project);
                await _ctx.SaveChangesAsync();

                await _eventLogService.LogEventAsync(
                    userId!,
                    "Project Edited",
                    $"Project '{project.Title}' was edited",
                    project.ProjectId
                );

                if (!string.IsNullOrEmpty(project.SubmitterId))
                {
                    var projectSubmitter = await _userManager.FindByIdAsync(project.SubmitterId);
                    if (projectSubmitter?.Email != null)
                    {
                        await _mailerService.SendEmailAsync(
                            projectSubmitter.Email,
                            $"Project Updated: {project.Title}",
                            $"Your project '{project.Title}' has been updated."
                        );
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.ProjectId))
                    return NotFound();
                throw;
            }
            return RedirectToAction("Index", "Home");
        }
        return View(project);
    }

    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var project = await _ctx
            .Projects.Include(p => p.Submitter)
            .FirstOrDefaultAsync(m => m.ProjectId == id);

        if (project == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (project.SubmitterId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(project);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await _ctx
            .Projects.Include(p => p.Submitter)
            .FirstOrDefaultAsync(p => p.ProjectId == id); // Include Submitter
        if (project != null)
        {
            var userId = _userManager.GetUserId(User);
            if (project.SubmitterId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _eventLogService.LogEventAsync(
                userId!,
                "Project Deleted",
                $"Project '{project.Title}' was deleted",
                project.ProjectId
            );

            if (project.Submitter?.Email != null)
            {
                await _mailerService.SendEmailAsync(
                    project.Submitter.Email,
                    $"Project Deleted: {project.Title}",
                    $"Your project '{project.Title}' has been deleted."
                );
            }

            _ctx.Projects.Remove(project);
        }

        await _ctx.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }

    private bool ProjectExists(int id)
    {
        return _ctx.Projects.Any(e => e.ProjectId == id);
    }
}

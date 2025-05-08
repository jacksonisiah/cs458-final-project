using System.Security.Claims;
using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.MVC.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectAPIController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectAPIController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager
    )
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        return await _context.Projects.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        return project;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<ActionResult<Project>> PostProject(Project project)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        project.SubmitterId = userId;
        project.ProjectStatus = ProjectStatus.Submitted; // Default status
        project.Deadline = DateTime.SpecifyKind(project.Deadline, DateTimeKind.Utc);

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProject), new { id = project.ProjectId }, project);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> PutProject(int id, Project project)
    {
        if (id != project.ProjectId)
        {
            return BadRequest();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existingProject = await _context
            .Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (existingProject == null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin") && existingProject.SubmitterId != userId)
        {
            return Forbid();
        }

        if (!User.IsInRole("Admin"))
        {
            project.SubmitterId = existingProject.SubmitterId;
            project.ProjectStatus = existingProject.ProjectStatus;
        }

        project.Deadline = DateTime.SpecifyKind(project.Deadline, DateTimeKind.Utc);

        _context.Entry(project).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProjectExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && project.SubmitterId != userId)
        {
            return Forbid();
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProjectExists(int id)
    {
        return _context.Projects.Any(e => e.ProjectId == id);
    }
}

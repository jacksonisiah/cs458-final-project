using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.MVC.Controllers;

[Authorize]
public class BidsController(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
    : Controller
{
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var bid = await ctx
            .Bids.Include(b => b.Project)
            .Include(b => b.Bidder)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (bid == null)
            return NotFound();

        var userId = userManager.GetUserId(User);
        if (bid.BidderId != userId && bid.Project?.SubmitterId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(bid);
    }

    [Authorize(Roles = "Admin,Bidder")]
    public IActionResult Create(int? projectId)
    {
        var bid = new Bid { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddDays(30) };
        if (projectId.HasValue)
        {
            bid.ProjectId = projectId;
            ViewData["ProjectId"] = new SelectList(
                ctx.Projects.Where(p => p.ProjectId == projectId),
                "ProjectId",
                "Title"
            );
        }
        else
        {
            // Only show projects that the user hasn't bid on yet
            var userId = userManager.GetUserId(User);
            var availableProjects = ctx.Projects.Where(p => !p.Bids.Any(b => b.BidderId == userId));
            ViewData["ProjectId"] = new SelectList(availableProjects, "ProjectId", "Title");
        }

        return View(bid);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Bidder")]
    public async Task<IActionResult> Create(
        [Bind("Id,ProjectId,BidStatus,BidNote,Proposal,AttachmentFile,StartTime,EndTime")] Bid bid
    )
    {
        if (ModelState.IsValid)
        {
            if (bid.AttachmentFile is { Length: > 0 })
            {
                var uniqueFileName = Guid.NewGuid() + "_" + bid.AttachmentFile.FileName;
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads"
                );
                Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await bid.AttachmentFile.CopyToAsync(fileStream);
                }

                bid.AttachmentPath = "/uploads/" + uniqueFileName;
            }

            bid.BidderId = userManager.GetUserId(User);
            bid.SubmittedTime = DateTime.UtcNow;
            bid.BidStatus = BidStatus.Submitted;
            ctx.Add(bid);
            await ctx.SaveChangesAsync();

            if (bid.ProjectId.HasValue)
            {
                return RedirectToAction("Details", "Project", new { id = bid.ProjectId });
            }
            return RedirectToAction("Index", "Home");
        }

        if (bid.ProjectId.HasValue)
        {
            ViewData["ProjectId"] = new SelectList(
                ctx.Projects.Where(p => p.ProjectId == bid.ProjectId),
                "ProjectId",
                "Title",
                bid.ProjectId
            );
        }
        else
        {
            ViewData["ProjectId"] = new SelectList(
                ctx.Projects,
                "ProjectId",
                "Title",
                bid.ProjectId
            );
        }
        return View(bid);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var bid = await ctx.Bids.Include(b => b.Project).FirstOrDefaultAsync(b => b.Id == id);

        if (bid == null)
            return NotFound();

        var userId = userManager.GetUserId(User);
        if (bid.BidderId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(bid);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind(
            "Id,ProjectId,BidStatus,BidNote,Proposal,AttachmentPath,SubmittedTime,StartTime,EndTime,BidderId"
        )]
            Bid bid
    )
    {
        if (id != bid.Id)
            return NotFound();

        var userId = userManager.GetUserId(User);
        var existingBid = await ctx.Bids.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

        if (existingBid == null)
            return NotFound();

        if (!User.IsInRole("Admin"))
        {
            if (existingBid.BidderId != userId)
                return Forbid();

            bid.BidStatus = existingBid.BidStatus;
            bid.BidderId = existingBid.BidderId;
            bid.ProjectId = existingBid.ProjectId;
        }
        else // we are admin
        {
            if (!bid.ProjectId.HasValue && existingBid.ProjectId.HasValue)
            {
                bid.ProjectId = existingBid.ProjectId;
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                ctx.Update(bid);
                await ctx.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BidExists(bid.Id))
                    return NotFound();
                throw;
            }

            if (bid.ProjectId.HasValue)
            {
                return RedirectToAction("Details", "Project", new { id = bid.ProjectId });
            }
            return RedirectToAction("Index", "Home");
        }

        ViewData["ProjectId"] = new SelectList(ctx.Projects, "ProjectId", "Title", bid.ProjectId);
        return View(bid);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var bid = await ctx
            .Bids.Include(b => b.Project)
            .Include(b => b.Bidder)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (bid == null)
            return NotFound();

        var userId = userManager.GetUserId(User);
        if (bid.BidderId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(bid);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bid = await ctx.Bids.Include(b => b.Project).FirstOrDefaultAsync(b => b.Id == id);

        var projectIdForRedirect = bid?.ProjectId;

        if (bid != null)
        {
            var userId = userManager.GetUserId(User);
            if (bid.BidderId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            ctx.Bids.Remove(bid);
        }

        await ctx.SaveChangesAsync();
        if (projectIdForRedirect.HasValue)
        {
            return RedirectToAction("Details", "Project", new { id = projectIdForRedirect });
        }
        return RedirectToAction("Index", "Home");
    }

    private bool BidExists(int id)
    {
        return ctx.Bids.Any(e => e.Id == id);
    }
}

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
public class BidAPIController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BidAPIController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bid>>> GetBids(int? projectId)
    {
        if (projectId.HasValue)
        {
            return await _context.Bids.Where(b => b.ProjectId == projectId).ToListAsync();
        }
        return await _context.Bids.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Bid>> GetBid(int id)
    {
        var bid = await _context.Bids.Include(b => b.Bidder).FirstOrDefaultAsync(b => b.Id == id);

        if (bid == null)
        {
            return NotFound();
        }

        return bid;
    }

    [HttpPost]
    public async Task<ActionResult<Bid>> PostBid(Bid bid)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        bid.BidderId = userId;
        bid.SubmittedTime = DateTime.UtcNow;
        bid.BidStatus = BidStatus.Submitted;
        bid.StartTime = DateTime.SpecifyKind(bid.StartTime, DateTimeKind.Utc);
        bid.EndTime = DateTime.SpecifyKind(bid.EndTime, DateTimeKind.Utc);

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, bid);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBid(int id, Bid bid)
    {
        if (id != bid.Id)
        {
            return BadRequest();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existingBid = await _context.Bids.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

        if (existingBid == null)
        {
            return NotFound();
        }

        bool isAdmin = User.IsInRole("Admin");
        bool isProjectManager = User.IsInRole("ProjectManager");

        if (existingBid.BidderId != userId && !isAdmin && !isProjectManager)
        {
            return Forbid();
        }

        if (existingBid.BidderId == userId && !isAdmin && !isProjectManager)
        {
            bid.BidStatus = existingBid.BidStatus;
        }

        bid.BidderId = existingBid.BidderId;
        bid.SubmittedTime = existingBid.SubmittedTime;
        bid.StartTime = DateTime.SpecifyKind(bid.StartTime, DateTimeKind.Utc);
        bid.EndTime = DateTime.SpecifyKind(bid.EndTime, DateTimeKind.Utc);

        _context.Entry(bid).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BidExists(id))
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
    public async Task<IActionResult> DeleteBid(int id)
    {
        var bid = await _context.Bids.FindAsync(id);
        if (bid == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (bid.BidderId != userId && !User.IsInRole("Admin") && !User.IsInRole("ProjectManager"))
        {
            return Forbid();
        }

        _context.Bids.Remove(bid);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BidExists(int id)
    {
        return _context.Bids.Any(e => e.Id == id);
    }
}

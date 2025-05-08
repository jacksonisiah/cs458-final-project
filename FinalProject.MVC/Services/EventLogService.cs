using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.MVC.Services;

public class EventLogService(
    ApplicationDbContext context,
    ILogger<EventLogService> logger,
    UserManager<ApplicationUser> userManager
)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<EventLogService> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task LogEventAsync(
        string userId,
        string eventType,
        string eventDescription,
        int? projectId = null,
        int? bidId = null
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        var userDisplayName = user?.DisplayName ?? user?.UserName ?? user?.Email;

        var eventLog = new EventLog
        {
            UserId = userId,
            EventType = eventType,
            EventDescription = eventDescription,
            EventDate = DateTime.UtcNow,
            ProjectId = projectId,
            BidId = bidId,
        };

        _context.EventLogs.Add(eventLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Event logged: {EventType} for User {UserId} ({UserDisplayName}). Project: {ProjectId}, Bid: {BidId}. {EventDescription}",
            eventType,
            userId,
            userDisplayName,
            projectId,
            bidId,
            eventDescription
        );
    }
}

namespace FinalProject.MVC.Models;

public enum BidStatus
{
    Submitted, // Default status when a bid is created
    UnderReview, // In review
    Accepted, // Accepted state
    Rejected, // Rejected state
    InProgress, // Currently undergoing
    Completed, // Completed state
    Withdrawn, // Withdrawn by the bidder
}

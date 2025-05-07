namespace FinalProject.MVC.Models;

public enum ProjectStatus
{
    Submitted, // Default status when a project is created
    UnderReview, // In review
    Accepted, // Accepted state
    Rejected, // Rejected state
    InProgress, // Currently undergoing
    Completed, // Completed state
    Withdrawn, // Withdrawn by the pm
}

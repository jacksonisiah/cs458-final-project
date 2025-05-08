namespace FinalProject.MVC.Models
{
    public class EventLogViewModel
    {
        public DateTime EventDate { get; set; }
        public string UserDisplayName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
    }
}

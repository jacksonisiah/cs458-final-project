using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.MVC.Models;

public class EventLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("event_type")]
    [StringLength(255)]
    [Required]
    [Display(Name = "Event Type")]
    public string EventType { get; set; } = null!;

    [Column("event_description")]
    [StringLength(255)]
    public string EventDescription { get; set; } = null!;

    [Column("event_date")]
    public DateTime EventDate { get; set; }

    [Column("uid")]
    public string UserId { get; set; } = null!;

    [Column("project_id")]
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    [Column("bid_id")]
    public int? BidId { get; set; }
    public Bid? Bid { get; set; }
}

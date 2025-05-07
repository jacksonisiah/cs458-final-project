using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace FinalProject.MVC.Models;

public class Bid
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("project_id")]
    public int? ProjectId { get; set; }

    [Column("bidder_id")]
    public string? BidderId { get; set; }

    [Column("bid_status")]
    [StringLength(255)]
    public BidStatus BidStatus { get; set; } = BidStatus.Submitted;

    [Column("bid_note")]
    public string? BidNote { get; set; }

    [Column("proposal")]
    [StringLength(255)]
    public string? Proposal { get; set; }

    [NotMapped]
    public IFormFile? AttachmentFile { get; set; }

    [Column("attachment")]
    public string? AttachmentPath { get; set; }

    [Column("submitted_time")]
    public DateTime? SubmittedTime { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }

    public virtual Project? Project { get; set; }

    [ForeignKey("BidderId")]
    public virtual ApplicationUser? Bidder { get; set; }
}

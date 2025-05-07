using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinalProject.MVC.Attributes;

namespace FinalProject.MVC.Models;

[Table("Projects")]
public class Project
{
    [Key]
    [Column("id")]
    public int ProjectId { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("tags")]
    public string? Tags { get; set; }

    [Column("deadline")]
    [Deadline(ErrorMessage = "Must be a future date.")]
    public DateTime Deadline { get; set; }

    [Column("project_status")]
    public ProjectStatus ProjectStatus { get; set; } = ProjectStatus.Submitted;

    [Column("funding", TypeName = "money")]
    public decimal? Funding { get; set; }

    [Column("submitter_id")]
    public string? SubmitterId { get; set; }

    [ForeignKey("SubmitterId")]
    public virtual ApplicationUser? Submitter { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
}

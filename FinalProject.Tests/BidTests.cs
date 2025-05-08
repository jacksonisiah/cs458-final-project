using FinalProject.MVC.Models;

namespace FinalProject.Tests;

[TestFixture]
public class BidTests
{
    [Test]
    public void CreateBid_ShouldSetPropertiesCorrectly()
    {
        var bid = new Bid
        {
            Id = 1,
            ProjectId = 101,
            BidderId = "jackson",
            BidStatus = BidStatus.Submitted,
            BidNote = "test",
            Proposal = "test",
            SubmittedTime = DateTime.Now,
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(10),
        };

        Assert.That(bid.Id, Is.EqualTo(1));
        Assert.That(bid.ProjectId, Is.EqualTo(101));
        Assert.That(bid.BidderId, Is.EqualTo("jackson"));
        Assert.That(bid.BidStatus, Is.EqualTo(BidStatus.Submitted));
        Assert.That(bid.BidNote, Is.EqualTo("test"));
        Assert.That(bid.Proposal, Is.EqualTo("test"));
        Assert.That(bid.SubmittedTime, Is.LessThanOrEqualTo(DateTime.Now));
        Assert.That(bid.StartTime, Is.GreaterThan(DateTime.Now));
        Assert.That(bid.EndTime, Is.GreaterThan(bid.StartTime));
    }
}

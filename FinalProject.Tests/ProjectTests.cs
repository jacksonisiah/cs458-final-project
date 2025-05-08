using FinalProject.MVC.Models;

namespace FinalProject.Tests;

[TestFixture]
public class ProjectTests
{
    [Test]
    public void CreateProject_ShouldSetPropertiesCorrectly()
    {
        var project = new Project
        {
            ProjectId = 1,
            Title = "test",
            Description = "test",
            Tags = "test",
            Deadline = DateTime.Now.AddDays(30),
            ProjectStatus = ProjectStatus.Submitted,
            Funding = 5000.00m,
            SubmitterId = "jackson",
        };

        Assert.That(project.ProjectId, Is.EqualTo(1));
        Assert.That(project.Title, Is.EqualTo("test"));
        Assert.That(project.Description, Is.EqualTo("test"));
        Assert.That(project.Tags, Is.EqualTo("test"));
        Assert.That(project.Deadline, Is.GreaterThan(DateTime.Now));
        Assert.That(project.ProjectStatus, Is.EqualTo(ProjectStatus.Submitted));
        Assert.That(project.Funding, Is.EqualTo(5000.00m));
        Assert.That(project.SubmitterId, Is.EqualTo("jackson"));
    }
}

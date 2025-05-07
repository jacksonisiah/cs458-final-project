using System;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.MVC.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePicture { get; set; }
}

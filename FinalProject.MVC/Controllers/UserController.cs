using FinalProject.MVC.Data;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.MVC.Controllers;

public class UserController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext ctx
) : Controller
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _ctx = ctx;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Profile(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> EditProfile(string? id)
    {
        ApplicationUser? user;
        if (!string.IsNullOrEmpty(id) && User.IsInRole("Admin"))
        {
            user = await _userManager.FindByIdAsync(id);
        }
        else
        {
            user = await _userManager.GetUserAsync(User);
        }

        if (user == null)
        {
            return NotFound();
        }

        ViewData["AvailableRoles"] = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(user);
        ViewData["CurrentRole"] = userRoles.FirstOrDefault() ?? "";
        ViewData["EditingUserId"] = user.Id;

        return View(user);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(
        string? id,
        [Bind("DisplayName,Bio,ProfilePicture")] ApplicationUser model,
        string role
    )
    {
        ApplicationUser? userToUpdate;
        if (!string.IsNullOrEmpty(id) && User.IsInRole("Admin"))
        {
            userToUpdate = await _userManager.FindByIdAsync(id);
        }
        else
        {
            userToUpdate = await _userManager.GetUserAsync(User);
        }

        if (userToUpdate == null)
        {
            return NotFound();
        }

        model.Id = userToUpdate.Id;

        if (ModelState.IsValid)
        {
            userToUpdate.DisplayName = model.DisplayName;
            userToUpdate.Bio = model.Bio;
            userToUpdate.ProfilePicture = model.ProfilePicture;

            var result = await _userManager.UpdateAsync(userToUpdate);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(role) && User.IsInRole("Admin"))
                {
                    var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
                    }

                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                    await _userManager.AddToRoleAsync(userToUpdate, role);
                }
                else if (
                    !string.IsNullOrEmpty(role)
                    && userToUpdate.Id == _userManager.GetUserId(User)
                )
                {
                    var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                    if (currentRoles.FirstOrDefault() != role)
                    {
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
                        }
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(role));
                        }
                        await _userManager.AddToRoleAsync(userToUpdate, role);
                    }
                }
                return RedirectToAction(nameof(Profile), new { id = userToUpdate.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewData["AvailableRoles"] = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        ViewData["CurrentRole"] = role;
        // Pass the user ID to the view again in case of error
        ViewData["EditingUserId"] = userToUpdate.Id;
        return View(model);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly UserManagementService _users;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AdminController(UserManagementService users, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _users = users;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Users()
    {
        var users = await _users.GetAllUsersAsync();
        ViewBag.AllRoles = RoleNames.All;
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Block(string userId)
    {
        var (success, error) = await _users.BlockAsync(userId);
        if (!success) return BadRequest(new { error });
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> Unblock(string userId)
    {
        var (success, error) = await _users.UnblockAsync(userId);
        if (!success) return BadRequest(new { error });
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string userId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId) return BadRequest(new { error = "You cannot delete your own account." });

        var (success, error) = await _users.DeleteAsync(userId);
        if (!success) return BadRequest(new { error });
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        var (success, error) = await _users.AssignRoleAsync(userId, roleName);
        if (!success) return BadRequest(new { error });
        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
        {
            var currentUser = await _userManager.FindByIdAsync(userId);
            if(currentUser != null) await _signInManager.RefreshSignInAsync(currentUser);
        }
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        var (success, error) = await _users.RemoveRoleAsync(userId, roleName);
        if (!success) return BadRequest(new { error });

        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
        {
            var currentUser = await _userManager.FindByIdAsync(userId);
            if(currentUser != null) await _signInManager.RefreshSignInAsync(currentUser);
        }
        return Ok();
    }
}
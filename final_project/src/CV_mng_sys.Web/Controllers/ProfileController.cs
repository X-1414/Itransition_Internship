using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

[Authorize] 
public class ProfileController : Controller
{
    private readonly CandidateProfileService _profile;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(CandidateProfileService profile, UserManager<ApplicationUser> userManager)
    {
        _profile = profile;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? userId = null)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        var targetUserid = currentUserId;
        if(!string.IsNullOrEmpty(userId) && userId != currentUserId)
        {
            if(!User.IsInRole("Administrator")) return Forbid();
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser is null) return NotFound();
            
            targetUserid = userId;
            ViewBag.ViewingAsAdmin = true;
            ViewBag.TargetEmail = targetUser.Email;
        }
        var attributes = await _profile.GetProfileAttributesAsync(targetUserid);
        ViewBag.TargetUserId = targetUserid;
        return View(attributes);
    }

    [HttpPost]
    public async Task<IActionResult> SetValue(int attributeDefinitionId, string? value, uint expectedVersion, string? userId = null)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        var targetUserid = currentUserId;
        if(!string.IsNullOrEmpty(userId) && userId != currentUserId)
        {
            if(!User.IsInRole("Administrator")) return Forbid();
            targetUserid = userId;
        }
        var (success, error, newVersion) = await _profile.SetValueAsync(targetUserid, attributeDefinitionId, value, expectedVersion);
        if (!success) return Conflict(new { error });
        return Ok(new { newVersion });
    }
}
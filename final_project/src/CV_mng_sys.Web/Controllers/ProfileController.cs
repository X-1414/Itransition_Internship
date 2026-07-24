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

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var attributes = await _profile.GetProfileAttributesAsync(userId);
        return View(attributes);
    }

    [HttpPost]
    public async Task<IActionResult> SetValue(int attributeDefinitionId, string? value, uint expectedVersion)
    {
        var userId = _userManager.GetUserId(User)!;
        var (success, error, newVersion) = await _profile.SetValueAsync(userId, attributeDefinitionId, value, expectedVersion);
        if (!success)
        {
            if (error == "This value was changed elsewhere. Please reload.")
                return Conflict(new { error });
            return BadRequest(new { error });
        }
        return Ok(new { newVersion });
    }
}
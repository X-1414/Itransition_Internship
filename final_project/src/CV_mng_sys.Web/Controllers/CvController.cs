using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

[Authorize]
public class CvController : Controller
{
    private readonly CvService _cvs;
    private readonly CandidateProfileService _profile;
    private readonly UserManager<ApplicationUser> _userManager;

    public CvController(CvService cvs, CandidateProfileService profile, UserManager<ApplicationUser> userManager)
    {
        _cvs = cvs;
        _profile = profile;
        _userManager = userManager;
    }

    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> MyCvs()
    {
        var userId = _userManager.GetUserId(User)!;
        var cvs = await _cvs.GetMyCvsAsync(userId);
        return View(cvs);
    }

    [Authorize(Roles = "Candidate")]
    [HttpPost]
    public async Task<IActionResult> Generate(int positionId)
    {
        var userId = _userManager.GetUserId(User)!;
        var cv = await _cvs.GetOrCreateAsync(userId, positionId);
        return RedirectToAction(nameof(Details), new { id = cv.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var cv = await _cvs.GetByIdAsync(id);
        if (cv is null) return NotFound();
        
        var userId = _userManager.GetUserId(User);
        bool isOwner = cv.CandidateUserId == userId;
        bool isAdmin = User.IsInRole("Administrator");
        bool isRecruiter = User.IsInRole("Recruiter");
        if (!isOwner && !isAdmin && !(isRecruiter && cv.Status == CvStatus.Published)) return Forbid();
        
        ViewBag.IsOwner = isOwner || isAdmin; // Admin can edit like the owner
        ViewBag.CanPublish = await _cvs.CanPublishAsync(cv.PositionId, cv.CandidateUserId);
        ViewBag.Rows = await _cvs.GetAttributeRowsAsync(cv.PositionId, cv.CandidateUserId);
        return View(cv);
    }

    [HttpPost]
    public async Task<IActionResult> SetValue(int cvId, int attributeDefinitionId, string? value, uint expectedVersion)
    {
        var cv = await _cvs.GetByIdAsync(cvId);
        if (cv is null) return NotFound();

        var userId = _userManager.GetUserId(User);
        bool canEdit = cv.CandidateUserId == userId || User.IsInRole("Administrator");
        if (!canEdit) return Forbid();

        var (success, error, newVersion) = await _profile.SetValueAsync(cv.CandidateUserId, attributeDefinitionId, value, expectedVersion);
        if (!success)
        {
            if (error == "This value was changed elsewhere. Please reload.")
                return Conflict(new { error });
            return BadRequest(new { error });
        }
        return Ok(new { newVersion });
    }

    [HttpPost]
    public async Task<IActionResult> Publish(int id, uint expectedVersion)
    {
        var cv = await _cvs.GetByIdAsync(id);
        if (cv is null) return NotFound();
        var userId = _userManager.GetUserId(User);
        if (cv.CandidateUserId != userId && !User.IsInRole("Administrator")) return Forbid();

        var (success, error) = await _cvs.PublishAsync(id, expectedVersion);
        if (!success) return Conflict(new { error });
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Unpublish(int id, uint expectedVersion)
    {
        var cv = await _cvs.GetByIdAsync(id);
        if (cv is null) return NotFound();
        var userId = _userManager.GetUserId(User);
        if (cv.CandidateUserId != userId && !User.IsInRole("Administrator")) return Forbid();

        var (success, error) = await _cvs.UnpublishAsync(id, expectedVersion);
        if (!success) return Conflict(new { error });
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, uint expectedVersion)
    {
        var cv = await _cvs.GetByIdAsync(id);
        if (cv is null) return NotFound();
        var userId = _userManager.GetUserId(User);
        if (cv.CandidateUserId != userId && !User.IsInRole("Administrator")) return Forbid();

        var (success, error) = await _cvs.DeleteAsync(id, expectedVersion);
        if (!success) return Conflict(new { error });
        return Ok();
    }
}
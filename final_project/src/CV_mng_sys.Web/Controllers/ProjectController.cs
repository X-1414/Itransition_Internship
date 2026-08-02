using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

[Authorize(Roles = "Candidate, Administrator")]
public class ProjectsController : Controller
{
    private readonly ProjectService _projects;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(ProjectService projects, UserManager<ApplicationUser> userManager)
    {
        _projects = projects;
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
        ViewBag.TargetUserId = targetUserid;
        return View(await _projects.GetForCandidateAsync(targetUserid));
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, DateOnly? startDate, DateOnly? endDate, string? descriptionMarkdown, string? tagsRaw, string? userId=null)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new {error = "Name is required."});
        var currentUserId = _userManager.GetUserId(User)!;
        var targetUserid = currentUserId;
        if(!string.IsNullOrEmpty(userId) && userId != currentUserId)
        {
            if(!User.IsInRole("Administrator")) return Forbid();
            targetUserid = userId;
        }
        var created = await _projects.CreateAsync(targetUserid, name, startDate, endDate, descriptionMarkdown, tagsRaw);
        return Ok(new {created.Id, created.Version});
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, DateOnly? startDate, DateOnly? endDate, string? descriptionMakrdown, string? tagsRaw, uint expectedVersion)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project is null) return NotFound();
        var currentUserId = _userManager.GetUserId(User)!;
        if(project.CandidateUserId != currentUserId && !User.IsInRole("Administrator")) return Forbid();
        
        var (success, error) = await _projects.UpdateAsync(id, name, startDate, endDate, descriptionMakrdown, tagsRaw, expectedVersion);
        if(!success) return Conflict (new { error });
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Delete (int id, uint expectedVersion)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project is null) return NotFound();
        var currentUserId = _userManager.GetUserId(User);
        if (project.CandidateUserId != currentUserId && !User.IsInRole("Administrator")) return Forbid();

        var (success, error) = await _projects.DeleteAsync(id, expectedVersion);
        if(!success) return Conflict (new { error });
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> TagsJson()
    {
        return Ok(await _projects.GetAllDistinctTagsAsync());
    }
}
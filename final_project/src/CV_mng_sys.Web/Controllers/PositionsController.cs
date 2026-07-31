using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Services;
using CV_mng_sys.Core.Entities;
using System.Security.Claims;
using Microsoft.CodeAnalysis.CSharp;

namespace CV_mng_sys.Web.Controllers;

public class PositionsController : Controller
{
    private readonly PositionService _positions;
    private readonly AttributeLibraryService _attributes;
    private readonly CvService _cvs;

    public PositionsController(PositionService positions, AttributeLibraryService attributes, CvService cvs)
    {
        _positions = positions;
        _attributes = attributes;
        _cvs = cvs;
    }
    public async Task<IActionResult> Index()
    {
        return View(await _positions.GetAllAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var position = await _positions.GetByIdAsync(id);
        if (position is null) return NotFound();
        if (User.IsInRole("Recruiter") || User.IsInRole("Administrator"))
        {
            ViewBag.Cvs = User.IsInRole("Administrator") ? await _cvs.GetAllForPositionAsync(id) : await _cvs.GetPublishedForPositionAsync(id);
        }
        if (User.IsInRole("Candidate"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            ViewBag.CandidateHasAccess = await _positions.CandidateHasAccessAsync(id, userId);
            ViewBag.UnmetAttributes = await _positions.GetUnmetRequirementAttributeNamesAsync(id, userId);
        }
        return View(position);
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> Create(string title, string? description, string? projectTagsRaw, int maxProjectsInCv)
    {
        if (string.IsNullOrWhiteSpace(title)) return BadRequest(new { error = "Title is required." });
        var created = await _positions.CreateAsync(title, description, projectTagsRaw, maxProjectsInCv);
        return Ok(new { created.Id, created.Title, created.Description, created.Version });
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> Update(int id, string title, string? description, string? projectTagsRaw, int maxProjectsInCv, uint expectedVersion)
    {
        var (success, error) = await _positions.UpdateAsync(id, title, description, projectTagsRaw, maxProjectsInCv, expectedVersion);
        if (!success) return Conflict(new { error });
        return Ok();
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id, uint expectedVersion)
    {
        var (success, error) = await _positions.DeleteAsync(id, expectedVersion);
        if (!success) return Conflict(new { error });
        return Ok();
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        var copy = await _positions.DuplicateAsync(id);
        return Ok(new { copy.Id });
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> AddAttribute(int positionId, int attributeDefinitionId, bool isRequired)
    {
        await _positions.AddAttributeAsync(positionId, attributeDefinitionId, isRequired);
        return RedirectToAction(nameof(Details), new { id = positionId });
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> RemoveAttribute(int positionAttributeId, int positionId)
    {
        await _positions.RemoveAttributeAsync(positionAttributeId);
        return RedirectToAction(nameof(Details), new { id = positionId });
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpGet]
    public async Task<IActionResult> AccessRulesJson(int positionId)
    {
        var rules = await _positions.GetAccessRulesAsync(positionId);
        return Ok(rules.Select(r => new { r.AttributeDefinitionId, OperatorValue = (int)r.Operator, r.ComparisonValue }));
    }

    public record AccessRuleDto(int AttributeDefinitionId, int OperatorValue, string? ComparisonValue);
    public record SaveAccessRulesRequest(int PositionId, List<AccessRuleDto> Rules);

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> SaveAccessRules([FromBody] SaveAccessRulesRequest request)
    {
        var rules = request.Rules.Select(r => (r.AttributeDefinitionId, (CV_mng_sys.Core.Entities.AccessRuleOperator)r.OperatorValue, r.ComparisonValue)).ToList();
        await _positions.SetAccessRulesAsync(request.PositionId, rules);
        return Ok();
    }
}
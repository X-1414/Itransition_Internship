using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

public class PositionsController : Controller
{
    private readonly PositionService _positions;
    private readonly AttributeLibraryService _attributes;

    public PositionsController(PositionService positions, AttributeLibraryService attributes)
    {
        _positions = positions;
        _attributes = attributes;
    }
    public async Task<IActionResult> Index()
    {
        return View(await _positions.GetAllAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var position = await _positions.GetByIdAsync(id);
        if (position is null) return NotFound();
        return View(position);
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> Create(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title)) return BadRequest(new { error = "Title is required." });
        var created = await _positions.CreateAsync(title, description);
        return Ok(new { created.Id, created.Title, created.Description, created.Version });
    }

    [Authorize(Roles = "Recruiter,Administrator")]
    [HttpPost]
    public async Task<IActionResult> Update(int id, string title, string? description, uint expectedVersion)
    {
        var (success, error) = await _positions.UpdateAsync(id, title, description, expectedVersion);
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
}
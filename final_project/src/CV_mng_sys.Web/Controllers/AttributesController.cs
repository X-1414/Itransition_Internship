using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CV_mng_sys.Core.Services;
using CV_mng_sys.Core.Entities;
using System.ComponentModel;

namespace CV_mng_sys.Web.Controllers;

[Authorize(Roles = "Recruiter, Administrator")]
public class AttributesController : Controller
{
    private readonly AttributeLibraryService _service;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AttributesController(AttributeLibraryService service, IStringLocalizer<CV_mng_sys.Web.SharedResource> localizer)
    {
        _service = service;
        _localizer = localizer;
    }

    // Get: Attributes
    public async Task<IActionResult> Index()
    {
        var attributes = await _service.GetAllAsync();
        return View(attributes);
    }

    // Post: Attributes/Create
    [HttpPost]
    public async Task<IActionResult> Create(string name, AttributeCategory category, string? description, AttributeDataType dataType, string? dropdownOptionsRaw)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Name is required." });

            var (success, error, created) = await _service.CreateAsync(name, category, description, dataType, dropdownOptionsRaw);
            if (!success) return BadRequest(new { error });
            return Ok(new { created!.Id, created.Name, DataType = created.DataType.ToString(), created.Version });
    }

    // Post: Attributes/Update
    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, AttributeCategory category, string? description, AttributeDataType dataType, string? dropdownOptionsRaw, uint expectedVersion)
    {
        var (success, error) = await _service.UpdateAsync(id, name, category, description, dataType, dropdownOptionsRaw, expectedVersion);
        if(!success)
        {
            bool isConflict = error == "This attribute was modified by someone else. Please reload and try again.";
            return isConflict ? Conflict(new { error }) : BadRequest(new { error });
        }
        var updated = await _service.GetByIdAsync(id);
        return Ok(new { updated!.Id, updated.Name, Category=updated.Category.ToString(), DataType = updated!.DataType.ToString(), updated!.Version });
    }

    // Post: Attributes/Delete
    [HttpPost]
    public async Task<IActionResult> Delete(int id, uint expectedVersion)
    {
        var (success, error) = await _service.DeleteAsync(id, expectedVersion);
        if (!success)
        {
            bool isConflict = error == "This attribute was modified by someone else. Please reload and try again.";
            return isConflict ? Conflict(new { error }) : BadRequest(new { error });
        }
        return Ok();
    }   

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ListJson()
    {
        var all = await _service.GetAllAsync();
        var recent = await _service.GetRecentlyUsedAsync(5);
        var recentIds = recent.Select(r=>r.Id).ToHashSet();

        return Ok(new {
            recentlyUsed = recent.Select(a => new { id=a.Id, name=a.Name, dataType = a.DataType.ToString(), dataTypeDisplay = _localizer[a.DataType.ToString()].Value }),
            all = all.Select(a => new { id=a.Id, name=a.Name, dataType = a.DataType.ToString(), dataTypeDisplay = _localizer[a.DataType.ToString()].Value, category=a.Category, IsRecent = recentIds.Contains(a.Id) })
        });
    }
}
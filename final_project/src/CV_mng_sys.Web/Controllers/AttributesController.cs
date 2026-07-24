using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Services;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Web.Controllers;

[Authorize(Roles = "Recruiter, Administrator")]
public class AttributesController : Controller
{
    private readonly AttributeLibraryService _service;

    public AttributesController(AttributeLibraryService service)
    {
        _service = service;
    }

    // Get: Attributes
    public async Task<IActionResult> Index()
    {
        var attributes = await _service.GetAllAsync();
        return View(attributes);
    }

    // Post: Attributes/Create
    [HttpPost]
    public async Task<IActionResult> Create(string name, AttributeDataType dataType, string? dropdownOptionsRaw)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Name is required." });

            var created = await _service.CreateAsync(name, dataType, dropdownOptionsRaw);
            return Ok(new { created.Id, created.Name, DataType = created.DataType.ToString(), created.Version });
    }

    // Post: Attributes/Update
    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, AttributeDataType dataType, string? dropdownOptionsRaw, uint expectedVersion)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Name is required." });

        var (success, error) = await _service.UpdateAsync(id, name, dataType, dropdownOptionsRaw, expectedVersion);
        if (!success)
            return Conflict(new { error });

        var updated = await _service.GetByIdAsync(id);
        return Ok(new { updated!.Id, updated.Name, DataType = updated!.DataType.ToString(), updated!.Version });
    }

    // Post: Attributes/Delete
    [HttpPost]
    public async Task<IActionResult> Delete(int id, uint expectedVersion)
    {
        var (success, error) = await _service.DeleteAsync(id, expectedVersion);
        if (!success)
            return Conflict(new { error });

        return Ok(new { message = "Attribute definition deleted successfully." });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ListJson()
    {
        var attrs = await _service.GetAllAsync();
        return Ok(attrs.Select(a => new { a.Id, a.Name, DataType = a.DataType.ToString() }));
    }
}
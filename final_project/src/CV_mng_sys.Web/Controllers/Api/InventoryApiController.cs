using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers.Api;

[ApiController]
[Route("api/inventories")]
public class InventoryApiController : ControllerBase
{
    private readonly PositionService _positions;
    private readonly CvService _cvs;

    public InventoryApiController(PositionService positions, CvService cvs)
    {
        _positions = positions;
        _cvs = cvs;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetInventory(string token)
    {
        var position = await _positions.GetByApiTokenAsync(token);
        if (position is null) return NotFound(new {error = "Invalid or unknown API token."});
        var aggregates = await _cvs.GetAggregatesForPositionAsync(position.Id);
        return Ok(new
        {
            inventoryTitle = position.Title,
            fields = position.Attributes.Select(pa => new{title = pa.AttributeDefinition.Name, type=pa.AttributeDefinition.DataType.ToString()}),
            aggregatedResults = aggregates.Select(a=> new
            {
                fieldName = a.FieldName,
                dataType = a.DataType,
                average = a.Average,
                min = a.Min,
                max = a.Max,
                topValues = a.TopValues,
            })
        });
    }
}
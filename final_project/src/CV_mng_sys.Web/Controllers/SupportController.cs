using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

[Authorize]
public class SupportController : Controller
{
    private readonly SupportTicketService _tickets;
    private readonly UserManager<ApplicationUser> _userManager;
    public SupportController(SupportTicketService tickets, UserManager<ApplicationUser> userManager)
    {
        _tickets = tickets;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Submit(string summary, string priority, string returnUrl, string? inventory)
    {
        if (string.IsNullOrWhiteSpace(summary) || string.IsNullOrWhiteSpace(priority))
        {
            return BadRequest(new {error = "Summary and priority are required."});
        }
        var email = User.Identity?.Name ?? _userManager.GetUserId(User) ?? "unknown";
        var fullReturnURl = $"{Request.Scheme}://{Request.Host}{returnUrl}";
        var ticket = new SupportTicketRequest(
            ReportedBy: email,
            Inventory: string.IsNullOrWhiteSpace(inventory) ? null : inventory,
            Link: fullReturnURl,
            Priority: priority,
            Summary: summary
        );
        var (success, error) = await _tickets.SubmitTicketAsync(ticket);
        if (!success) return BadRequest(new {error});
        return Ok();
    }
}
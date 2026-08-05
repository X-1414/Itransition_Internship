using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

[Authorize]
public class SalesforceController : Controller
{
    private readonly SalesforceService _salesforceService;
    private readonly UserManager<ApplicationUser> _userManager;
    public SalesforceController(SalesforceService salesforceService, UserManager<ApplicationUser> userManager)
    {
        _salesforceService = salesforceService;
        _userManager = userManager;
    }
    public async Task<IActionResult> Connect()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewBag.PrefilledEmail = user?.Email ?? "";
        ViewBag.AlreadyConnected = !string.IsNullOrEmpty(user?.SalesforceAccountId);
        ViewBag.ExistingAccountId = user?.SalesforceAccountId;
        ViewBag.ExistingContactId = user?.SalesforceContactId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Connect(string companyName, string firstName, string lastName, string email, string? phone, string? title)
    {
        if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
        {
            ViewBag.Error = "Company name, first name, last name, and email are required.";
            ViewBag.PrefilledEmail = email;
            return View();
        }
        var request = new SalesforceAccountContactRequest(companyName, firstName, lastName, email, phone, title);
        var (success, error, accountId, contactId) = await _salesforceService.CreateAccountWithContactAsync(request);
        if (!success)
        {
            ViewBag.Error = error;
            ViewBag.PrefilledEmail = email;
            return View();
        }
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            user.SalesforceAccountId = accountId;
            user.SalesforceContactId = contactId;
            await _userManager.UpdateAsync(user);
        }
        ViewBag.Success = true;
        ViewBag.AccountId = accountId;
        ViewBag.ContactId = contactId;
        return View();
    }
}
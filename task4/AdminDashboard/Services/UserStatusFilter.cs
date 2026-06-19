// before every request except login/register check if the user still exists in the db and isn't blocked. 

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AdminDashboard.Models;
using AdminDashboard.Controllers;

namespace AdminDashboard.Services;

public class UserStatusFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;

    public UserStatusFilter(AppDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var descriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        var controllerName = descriptor?.ControllerName;

        if (controllerName == "Account")
        {
            await next();
            return;
        }

        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");

        if (userId == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }
        
        var user = await _db.Users.FindAsync(userId.Value);
        if (user == null || user.Status == UserStatus.Blocked)
        {
            session.Clear();
            context.Result = new RedirectToActionResult("Login", "Account", new{reason = user == null ? "deleted" : "blocked" });
            return;
        }

        if (user.LastLoginAt == null || (DateTime.UtcNow - user.LastLoginAt.Value).TotalSeconds > 60)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        
        await next();
    }
}
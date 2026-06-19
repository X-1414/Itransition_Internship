// table, block/unblock/delete/delete_unverified
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminDashboard.Models;

namespace AdminDashboard.Controllers;
public class UsersController : Controller
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db=db;

    // GET users
    public async Task<IActionResult> Index(string? status, string? name, string? email)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var statusEnum))
        {
            query = query.Where(u=>u.Status==statusEnum);
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            var nameTerm = name.Trim().ToLower();
            query = query.Where(u=>u.Name.ToLower().Contains(nameTerm));
        }
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailTerm = email.Trim().ToLower();
            query = query.Where(u=>u.Email.ToLower().Contains(emailTerm));
        }
        var users = await query.OrderByDescending(u=>u.LastLoginAt).ThenByDescending(u=>u.RegisteredAt).ToListAsync();
        var rows = users.Select(u=>new UserViewModel
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Status = u.Status,
            LastLoginAt = u.LastLoginAt,
            RegisteredAt = u.RegisteredAt,
        }).ToList();

        ViewData["FilterStatus"] = status ?? "";
        ViewData["FilterName"] = name ?? "";
        ViewData["FilterEmail"] = email ?? "";

        return View(rows);
    }

    // POST users/block
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Block([FromForm] List<int> selectedIds)
    {
        if (!selectedIds.Any())
        {
            TempData["Warning"] = "No users selected.";
            return RedirectToAction("Index");
        }
        var users = await _db.Users.Where(u=>selectedIds.Contains(u.Id)).ToListAsync();
        foreach (var u in users)
        {
            u.Status = UserStatus.Blocked;
        }
        await _db.SaveChangesAsync(); 

        TempData["Success"] = $"{users.Count} user(s) blocked.";

        // if the user blocked themselves
        var myId = HttpContext.Session.GetInt32("UserId");
        if (myId.HasValue && selectedIds.Contains(myId.Value))
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account", new { reason = "blocked" });
        }

        return RedirectToAction("Index");
    }

    // POST users/unblock
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock([FromForm] List<int> selectedIds)
    {
        if (!selectedIds.Any())
        {
            TempData["Warning"] = "No users selected.";
            return RedirectToAction("Index");
        }
        var users = await _db.Users.Where(u=>selectedIds.Contains(u.Id) && u.Status == UserStatus.Blocked).ToListAsync();
        foreach (var u in users) u.Status = u.WasEverVerified ? UserStatus.Active : UserStatus.Unverified;

        await _db.SaveChangesAsync(); 

        TempData["Success"] = users.Count > 0 ? $"{users.Count} user(s) unblocked." : "No blocked users were in the selection.";

        return RedirectToAction("Index");
    }

    // POST users/delete
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] List<int> selectedIds)
    {
        if (!selectedIds.Any())
        {
            TempData["Warning"] = "No users selected.";
            return RedirectToAction("Index");
        }

        var myId = HttpContext.Session.GetInt32("UserId");
        var users = await _db.Users.Where(u=>selectedIds.Contains(u.Id)).ToListAsync();
        bool selfDeleted = myId.HasValue && selectedIds.Contains(myId.Value);

        _db.Users.RemoveRange(users);
        await _db.SaveChangesAsync();

        if (selfDeleted)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account", new { reason = "deleted" });
        }

        TempData["Success"] = $"{users.Count} user(s) deleted.";
        return RedirectToAction("Index");
    }

    // POST users/delete_unverified
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnverified()
    {
        var myId = HttpContext.Session.GetInt32("UserId");
        var unverified = await _db.Users.Where(u=>u.Status==UserStatus.Unverified).ToListAsync();
        bool selfDeleted = myId.HasValue && unverified.Any(u=>u.Id == myId.Value);

        _db.Users.RemoveRange(unverified);
        await _db.SaveChangesAsync();

        if (selfDeleted)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account", new { reason = "deleted" });
        }

        TempData["Success"] = $"{unverified.Count} unverified user(s) deleted.";
        return RedirectToAction("Index");
    }
}
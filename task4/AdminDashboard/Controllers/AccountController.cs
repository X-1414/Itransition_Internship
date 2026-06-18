// login, register, verify email, logout

using Microsoft.AspNetCore.Mvc;
using AdminDashboard.Models;
using AdminDashboard.Services;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers;
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AccountController(AppDbContext db, IEmailService emailService, IConfiguration config)
    {
        _db = db;
        _emailService = emailService;
        _config = config;
    }

    // GET account/login
    public IActionResult Login(string? reason, string? prefillEmail){
        if (HttpContext.Session.GetInt32("UserId") != null)
            return RedirectToAction("Index", "Users");
        if (reason == "deleted")
            TempData["Error"] = "Your account has been deleted. Please register again.";
        else if (reason == "blocked")
            TempData["Error"] = "Your account has been blocked. Please contact an administrator or sign up with a new account.";

        var model = new LoginViewModel();
        if(!string.IsNullOrWhiteSpace(prefillEmail))
            model.Email = prefillEmail;

        return View(model);
    }

    // POST account/login
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if(!ModelState.IsValid) return View(model);

        var user = _db.Users.FirstOrDefault(u => u.Email == model.Email.ToLower().Trim());

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }
     
        if (user.Status == UserStatus.Blocked)
        {
            ModelState.AddModelError(string.Empty, "This account is blocked.");
            return View(model);
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.CurrentSessionStartUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);

        return RedirectToAction("Index", "Users");
    }


    // GET account/register
    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32("UserId") != null)
            return RedirectToAction("Index", "Users");
        return View();
    }

    // POST account/register
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register (RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var emailNorm = model.Email.ToLower().Trim();

        if (await _db.Users.AnyAsync(u => u.Email == emailNorm))
        {
            ModelState.AddModelError("Email", "This email address is already registered.");
            return View(model);
        }
        var token = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Name = model.Name.Trim(),
            Email = emailNorm,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Status = UserStatus.Unverified,
            RegisteredAt = DateTime.UtcNow,
            EmailVerificationToken = token,
            WasEverVerified = false
        };

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        // PostgreSQL error 23505 - unique violation - raised by ix-users-email-unique
        catch (DbUpdateException dbEx) when (dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            ModelState.AddModelError("Email", "This email address is already registered. (Rejected by the database's unique index.)");
            return View(model);
        }

        //send ver email async, don't block registration
        var baseUrl = _config["AppSettings:BaseUrl"]?.TrimEnd('/');
        var verifyLink = $"{baseUrl}/Account/Verify?token={token}";
        _ = Task.Run(async() => await _emailService.SendVerificationEmailAsync(user.Email, user.Name, verifyLink));

        TempData["Success"] = $"Registration was successful! Welcome, {user.Name}. " + "A verification link has been sent to your email.";
        return RedirectToAction("Login");
    }

    // GET account/verify
    public async Task<IActionResult> Verify(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Invalid verification link.";
            return RedirectToAction("Login");
        }

        var user = _db.Users.FirstOrDefault(u=>u.EmailVerificationToken == token);
        if (user == null)
        {
            TempData["Error"] = "Verification link is invalid or has already been used.";
            return RedirectToAction("Login");
        }

        if (user.Status != UserStatus.Blocked)
            user.Status = UserStatus.Active;

        user.WasEverVerified = true;
        user.EmailVerificationToken = null;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Email verified. Your account is now active.";

        //open in the same tab
        var sessionUserId = HttpContext.Session.GetInt32("UserId");
        if (sessionUserId == user.Id)
            return RedirectToAction("Index", "Users");

        return RedirectToAction("Login", new{prefillEmail = user.Email});
    }


    // POST account/logout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId.HasValue)
        {
            var user = await _db.Users.FindAsync(userId.Value);
            if (user != null)
            {
                FinalizeSession(user);
                await _db.SaveChangesAsync();
            }
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public static void FinalizeSession(User user)
    {
        if (user.CurrentSessionStartUnix == null) return;

        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var durationMinutes = (int)Math.Max(1, (nowUnix - user.CurrentSessionStartUnix.Value)/60);

        var entries = string.IsNullOrWhiteSpace(user.ActivityLog) ? new List<string>() : user.ActivityLog.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

        entries.Add($"{user.CurrentSessionStartUnix.Value}:{durationMinutes}");

        if (entries.Count>7) entries = entries.Skip(entries.Count-7).ToList();
        user.ActivityLog=string.Join(';', entries);
        user.CurrentSessionStartUnix = null;
    }
}
    
 



    
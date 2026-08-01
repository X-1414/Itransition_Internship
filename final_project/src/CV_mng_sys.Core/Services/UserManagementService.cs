using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Core.Services;

public record UserSummary(string Id, string Email, List<string> Roles, bool IsBlocked);
public class UserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UserManagementService(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<List<UserSummary>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<UserSummary>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            bool isBlocked = await _userManager.IsLockedOutAsync(user);
            result.Add(new UserSummary(user.Id, user.Email ?? "", roles.ToList(), isBlocked));
        }
        return result;
    }

    public async Task<(bool Success, string? Error)> BlockAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        return (true, null);
    }
    public async Task<(bool Success, string? Error)> UnblockAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found");
        await _userManager.SetLockoutEndDateAsync(user, null);
        return (true, null);
    }
    public async Task<(bool Success, string? Error)> DeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (true, null);
        await _userManager.DeleteAsync(user);
        return (true, null);
    }
    public async Task<(bool Success, string? Error)> AssignRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found");

        if (await _userManager.IsInRoleAsync(user, roleName)) return (true, null);
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded ? (true, null) : (false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }
    public async Task<(bool Success, string? Error)> RemoveRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return (false, "User not found");

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded ? (true, null) : (false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
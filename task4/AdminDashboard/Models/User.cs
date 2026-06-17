using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models;

public enum UserStatus
{
    Unverified, Active, Blocked
}

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required, MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Unverified; //unverified/active/blocked 
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? EmailVerificationToken { get; set; }
    public bool WasEverVerified { get; set; } = false;
    public string? ActivityLog { get; set;}
    public long? CurrentSessionStartUnix { get; set; }
}


using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Name is required."), MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required."), MinLength(1, ErrorMessage = "Password must be at least 1 character.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class UserViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserStatus Status { get; set; } 
    public DateTime? LastLoginAt { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class UssersFilterViewModel
{
    public string? Status { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}


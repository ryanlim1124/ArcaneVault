// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.Dtos;

/// <summary>What the API accepts when a visitor registers a new account.</summary>
public class RegisterInputDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    // [EmailAddress] enforces the "correct type" validation requirement - a value like
    // "notanemail" is rejected with a 400 before any database work happens.
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>What the API accepts when an existing user signs in.</summary>
public class LoginInputDto
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// What the API returns once an account is created or authenticated.
/// Note it deliberately never contains the password hash.
/// </summary>
public class UserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

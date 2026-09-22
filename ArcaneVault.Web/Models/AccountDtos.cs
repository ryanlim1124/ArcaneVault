// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Web.Models;

/// <summary>
/// Bound by the Register page form. The same validation attributes exist on the API's
/// own DTO, so the rules are enforced twice: once in the browser by jQuery Unobtrusive
/// Validation, and again server-side by the API. The browser check is a convenience;
/// the API check is the one that actually protects the database.
/// </summary>
public class RegisterInputDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(50)]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(200)]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The two passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>Bound by the Login page form.</summary>
public class LoginInputDto
{
    [Required(ErrorMessage = "Username is required.")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>The account details returned by the API after registering or signing in.</summary>
public class UserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

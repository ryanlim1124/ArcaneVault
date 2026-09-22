// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.Models;

/// <summary>
/// A registered account on the platform - either a collector/shop owner ("User")
/// or an Arcane Vault administrator ("Staff").
/// </summary>
public class ArcaneVaultUser
{
    /// <summary>
    /// Primary key. Note this is a natural string key (the chosen username), not an
    /// auto-increment integer - the assignment schema specifies UserName as the PK.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Soft-delete flag. Deactivated accounts are retained (their collection items
    /// still reference them) but are excluded from logins and analytics.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>Foreign key to <see cref="ArcaneVaultUserRole"/>.</summary>
    public int RoleId { get; set; }
    public ArcaneVaultUserRole Role { get; set; } = null!;

    /// <summary>
    /// ADDED COLUMN (not in the original schema): the assignment requires account
    /// registration and login authentication, which is impossible without storing a
    /// credential. This holds a PBKDF2 hash produced by ASP.NET Core's PasswordHasher -
    /// the plain-text password is never stored anywhere.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Every collection item owned by this account (one-to-many).</summary>
    public ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();
}

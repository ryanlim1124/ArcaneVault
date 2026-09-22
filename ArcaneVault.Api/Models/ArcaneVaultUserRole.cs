// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.Models;

/// <summary>
/// Lookup table of the roles a user account can hold.
/// The prototype ships with exactly two: "User" (shop owners and collectors)
/// and "Staff" (Arcane Vault's administrators).
/// </summary>
public class ArcaneVaultUserRole
{
    /// <summary>Primary key. Seeded as 1 = User, 2 = Staff.</summary>
    public int RoleId { get; set; }

    [Required]
    [MaxLength(20)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>All accounts holding this role (one-to-many).</summary>
    public ICollection<ArcaneVaultUser> Users { get; set; } = new List<ArcaneVaultUser>();
}

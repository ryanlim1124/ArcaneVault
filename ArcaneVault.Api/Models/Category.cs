// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.Models;

/// <summary>
/// A collection category (e.g. "TCG" - Trading Card Games, "FIG" - Figurines).
/// This is the platform's master data, maintained exclusively by Staff accounts,
/// and is what keeps every collector's items classified consistently.
/// </summary>
public class Category
{
    /// <summary>
    /// Primary key. A short human-readable code (e.g. "TCG") rather than an
    /// auto-increment integer, as specified by the assignment schema.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string CategoryCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Join rows linking this category to collection items (many-to-many).</summary>
    public ICollection<CollectionItemCategory> CollectionItemCategories { get; set; } = new List<CollectionItemCategory>();

    /// <summary>Skip navigation: the items in this category, without going through the join entity.</summary>
    public ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();
}

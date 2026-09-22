// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.Models;

/// <summary>
/// Join entity resolving the many-to-many between <see cref="CollectionItem"/> and
/// <see cref="Category"/> - one collectible can sit in several categories, and each
/// category holds many collectibles.
///
/// This is declared as an EXPLICIT entity class (rather than letting EF Core generate
/// an implicit join table) because the assignment schema names the table and specifies
/// a composite primary key of (ItemId, CategoryCode). That composite key is configured
/// in AppDbContext.OnModelCreating and is what prevents the same category being added
/// to the same item twice.
/// </summary>
public class CollectionItemCategory
{
    /// <summary>First half of the composite primary key; foreign key to CollectionItem.</summary>
    public int ItemId { get; set; }
    public CollectionItem CollectionItem { get; set; } = null!;

    /// <summary>Second half of the composite primary key; foreign key to Category.</summary>
    [Required]
    [MaxLength(10)]
    public string CategoryCode { get; set; } = string.Empty;
    public Category Category { get; set; } = null!;
}

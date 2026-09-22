// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.Dtos;

/// <summary>Read shape of a collection item, flattened for display.</summary>
public class CollectionItemDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int StartingQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>Category codes attached to this item (from the join table).</summary>
    public string[] CategoryCodes { get; set; } = [];

    /// <summary>Human-readable category names, for display as badges.</summary>
    public string[] CategoryNames { get; set; } = [];

    /// <summary>
    /// Units that have left this collection since acquisition
    /// (StartingQuantity - CurrentQuantity). The raw demand signal.
    /// </summary>
    public int UnitsMoved => StartingQuantity - CurrentQuantity;
}

/// <summary>Write shape used when a collector adds or updates an item.</summary>
public class CollectionItemInputDto
{
    [Required(ErrorMessage = "Item name is required.")]
    [MaxLength(150)]
    public string ItemName { get; set; } = string.Empty;

    [Range(0, 100_000, ErrorMessage = "Starting quantity must be between 0 and 100,000.")]
    public int StartingQuantity { get; set; }

    [Range(0, 100_000, ErrorMessage = "Current quantity must be between 0 and 100,000.")]
    public int CurrentQuantity { get; set; }

    /// <summary>
    /// The owning account. Set server-side from the signed-in user by the Web layer,
    /// so a collector can never create an item inside somebody else's collection.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    public string[] CategoryCodes { get; set; } = [];
}

/// <summary>Generic paged wrapper for list endpoints.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

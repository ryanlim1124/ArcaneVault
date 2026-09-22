// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Web.Models;

/// <summary>Mirrors ArcaneVault.Api.Dtos.CollectionItemDto.</summary>
public class CollectionItemDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int StartingQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string[] CategoryCodes { get; set; } = [];
    public string[] CategoryNames { get; set; } = [];

    /// <summary>Units that have left this collection since acquisition.</summary>
    public int UnitsMoved => StartingQuantity - CurrentQuantity;
}

/// <summary>Bound by the collection item Create and Edit forms.</summary>
public class CollectionItemInputDto
{
    [Required(ErrorMessage = "Item name is required.")]
    [MaxLength(150)]
    [Display(Name = "Item name")]
    public string ItemName { get; set; } = string.Empty;

    [Range(0, 100_000, ErrorMessage = "Starting quantity must be between 0 and 100,000.")]
    [Display(Name = "Starting quantity")]
    public int StartingQuantity { get; set; }

    [Range(0, 100_000, ErrorMessage = "Current quantity must be between 0 and 100,000.")]
    [Display(Name = "Current quantity")]
    public int CurrentQuantity { get; set; }

    /// <summary>Set server-side from the signed-in user - never posted by the browser.</summary>
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Categories")]
    public string[] CategoryCodes { get; set; } = [];
}

/// <summary>Mirrors ArcaneVault.Api.Dtos.PagedResult&lt;T&gt;.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

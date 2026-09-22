// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;


//Plain c# class that represents a single row of my database table,
//for example it holds the starting quantity and currentquantity properties
//entity Framework core uses this file to know exactly what columns to read when the
//InsightsController runs its calculations


namespace ArcaneVault.Api.Models;


public class CollectionItem
{
    public int ItemId { get; set; }

    [Required]
    [MaxLength(150)]
    public string ItemName { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    [Range(0, 100_000)]
    public int StartingQuantity { get; set; }

  
    [Range(0, 100_000)]
    public int CurrentQuantity { get; set; }

    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    public ArcaneVaultUser User { get; set; } = null!;

   
    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public ICollection<CollectionItemCategory> CollectionItemCategories { get; set; } = new List<CollectionItemCategory>();

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}

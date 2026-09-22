// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Api.Dtos;

/// <summary>Read shape of a Category, including how many items currently use it.</summary>
public class CategoryDto
{
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// How many collection items are classified under this category. Shown on the
    /// Staff list/details pages so an administrator can see the impact of a change
    /// before editing or deleting.
    /// </summary>
    public int ItemCount { get; set; }
}

/// <summary>Write shape used when Staff create or update a Category.</summary>
public class CategoryInputDto
{
    [Required(ErrorMessage = "Category code is required.")]
    [MaxLength(10, ErrorMessage = "Category code cannot exceed 10 characters.")]
    public string CategoryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;
}

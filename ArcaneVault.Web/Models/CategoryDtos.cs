// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Web.Models;

/// <summary>Mirrors ArcaneVault.Api.Dtos.CategoryDto.</summary>
public class CategoryDto
{
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

/// <summary>Bound by the Category Create and Edit forms.</summary>
public class CategoryInputDto
{
    [Required(ErrorMessage = "Category code is required.")]
    [MaxLength(10, ErrorMessage = "Category code cannot exceed 10 characters.")]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Category code may only contain letters and numbers.")]
    [Display(Name = "Category code")]
    public string CategoryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;
}

// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Api.Data;
using ArcaneVault.Api.Dtos;
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

/// <summary>
/// Full CRUD over the platform's master list of collection categories.
/// Only Staff accounts reach these endpoints - that restriction is enforced by the
/// Razor front end, whose Category pages carry [Authorize(Roles = "Staff")].
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(AppDbContext db, ILogger<CategoriesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/categories
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        // Project straight into the DTO, counting linked items in the same query rather
        // than loading every join row into memory just to count it.
        var categories = await _db.Categories
            .OrderBy(c => c.CategoryCode)
            .Select(c => new CategoryDto
            {
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                ItemCount = c.CollectionItemCategories.Count(cic => !cic.CollectionItem.IsDeleted),
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET api/categories/TCG
    [HttpGet("{categoryCode}")]
    public async Task<ActionResult<CategoryDto>> GetByCode(string categoryCode)
    {
        var category = await _db.Categories
            .Where(c => c.CategoryCode == categoryCode)
            .Select(c => new CategoryDto
            {
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                ItemCount = c.CollectionItemCategories.Count(cic => !cic.CollectionItem.IsDeleted),
            })
            .FirstOrDefaultAsync();

        if (category is null)
        {
            return NotFound(new { message = $"Category '{categoryCode}' was not found." });
        }

        return Ok(category);
    }

    // POST api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryInputDto input)
    {
        // CategoryCode is the primary key, so a duplicate would throw at the database.
        // Checking first lets us return a clear, actionable message instead.
        var code = input.CategoryCode.Trim().ToUpperInvariant();

        if (await _db.Categories.AnyAsync(c => c.CategoryCode == code))
        {
            return BadRequest(new { message = $"A category with the code '{code}' already exists." });
        }

        if (await _db.Categories.AnyAsync(c => c.CategoryName == input.CategoryName))
        {
            return BadRequest(new { message = $"A category named '{input.CategoryName}' already exists." });
        }

        var category = new Category
        {
            // Codes are normalised to upper case so "tcg" and "TCG" can never both exist.
            CategoryCode = code,
            CategoryName = input.CategoryName.Trim(),
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created category {CategoryCode}", category.CategoryCode);

        return CreatedAtAction(nameof(GetByCode), new { categoryCode = category.CategoryCode }, new CategoryDto
        {
            CategoryCode = category.CategoryCode,
            CategoryName = category.CategoryName,
            ItemCount = 0,
        });
    }

    // PUT api/categories/TCG
    [HttpPut("{categoryCode}")]
    public async Task<IActionResult> Update(string categoryCode, CategoryInputDto input)
    {
        var category = await _db.Categories.FindAsync(categoryCode);
        if (category is null)
        {
            return NotFound(new { message = $"Category '{categoryCode}' was not found." });
        }

        // The code is the primary key and is referenced by the join table, so it is fixed
        // once created. Only the display name can be edited.
        if (!string.Equals(input.CategoryCode.Trim(), categoryCode, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "The category code cannot be changed once the category exists. Create a new category instead." });
        }

        if (await _db.Categories.AnyAsync(c => c.CategoryName == input.CategoryName && c.CategoryCode != categoryCode))
        {
            return BadRequest(new { message = $"A category named '{input.CategoryName}' already exists." });
        }

        category.CategoryName = input.CategoryName.Trim();
        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryCode}", categoryCode);

        return NoContent();
    }

    // DELETE api/categories/TCG
    [HttpDelete("{categoryCode}")]
    public async Task<IActionResult> Delete(string categoryCode)
    {
        var category = await _db.Categories.FindAsync(categoryCode);
        if (category is null)
        {
            return NotFound(new { message = $"Category '{categoryCode}' was not found." });
        }

        // The join table's foreign key uses Restrict, so the database would reject this
        // with an opaque constraint error. Checking up front produces a message that
        // actually tells the administrator what to do about it.
        var inUseCount = await _db.CollectionItemCategories.CountAsync(cic => cic.CategoryCode == categoryCode);
        if (inUseCount > 0)
        {
            return BadRequest(new
            {
                message = $"Cannot delete '{category.CategoryName}' because {inUseCount} collection item(s) still use it. Reassign those items first."
            });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryCode}", categoryCode);

        return NoContent();
    }
}

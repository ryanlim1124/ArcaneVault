// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Api.Data;
using ArcaneVault.Api.Dtos;
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

[ApiController]
[Route("api/collectionitems")]
public class CollectionItemsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CollectionItemsController> _logger;

    public CollectionItemsController(AppDbContext db, ILogger<CollectionItemsController> logger)
    {
        _db = db;
        _logger = logger;
    }
    private static IQueryable<CollectionItemDto> ProjectToDto(IQueryable<CollectionItem> query) =>
        query.Select(i => new CollectionItemDto
        {
            ItemId = i.ItemId,
            ItemName = i.ItemName,
            StartingQuantity = i.StartingQuantity,
            CurrentQuantity = i.CurrentQuantity,
            UserName = i.UserName,
            CreatedAt = i.CreatedAt,
            LastUpdatedAt = i.LastUpdatedAt,
            CategoryCodes = i.CollectionItemCategories.Select(cic => cic.CategoryCode).ToArray(),
            CategoryNames = i.CollectionItemCategories.Select(cic => cic.Category.CategoryName).ToArray(),
        });

    [HttpGet]
    public async Task<ActionResult<PagedResult<CollectionItemDto>>> GetAll(
        [FromQuery] string? userName,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 10 : pageSize;


        var query = _db.CollectionItems.Where(i => !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            query = query.Where(i => i.UserName == userName);
        }


        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var pattern = $"%{term}%";

           
            var isNumeric = int.TryParse(term, out var numericTerm);

            query = query.Where(i =>
                EF.Functions.Like(i.ItemName, pattern)
                || EF.Functions.Like(i.UserName, pattern)
                || i.CollectionItemCategories.Any(cic =>
                       EF.Functions.Like(cic.CategoryCode, pattern)
                    || EF.Functions.Like(cic.Category.CategoryName, pattern))
                || (isNumeric && (i.ItemId == numericTerm
                               || i.StartingQuantity == numericTerm
                               || i.CurrentQuantity == numericTerm)));
        }

        var totalCount = await query.CountAsync();

        var items = await ProjectToDto(
                query.OrderByDescending(i => i.LastUpdatedAt).ThenByDescending(i => i.ItemId))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<CollectionItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    // GET api/collectionitems/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CollectionItemDto>> GetById(int id)
    {
        var item = await ProjectToDto(_db.CollectionItems.Where(i => i.ItemId == id && !i.IsDeleted))
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return NotFound(new { message = $"Collection item {id} was not found." });
        }

        return Ok(item);
    }

    // POST api/collectionitems
    [HttpPost]
    public async Task<ActionResult<CollectionItemDto>> Create(CollectionItemInputDto input)
    {
        var validationError = await ValidateAsync(input);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var now = DateTime.UtcNow;
        var item = new CollectionItem
        {
            ItemName = input.ItemName.Trim(),
            StartingQuantity = input.StartingQuantity,
            CurrentQuantity = input.CurrentQuantity,
            UserName = input.UserName,
            IsDeleted = false,
            CreatedAt = now,
            LastUpdatedAt = now,
        };

        foreach (var code in input.CategoryCodes.Distinct())
        {
            item.CollectionItemCategories.Add(new CollectionItemCategory { CategoryCode = code });
        }

        _db.CollectionItems.Add(item);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created collection item {ItemId} for {UserName}", item.ItemId, item.UserName);

        var created = await ProjectToDto(_db.CollectionItems.Where(i => i.ItemId == item.ItemId)).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.ItemId }, created);
    }

    // PUT api/collectionitems/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CollectionItemInputDto input)
    {
        var item = await _db.CollectionItems
            .Include(i => i.CollectionItemCategories)
            .FirstOrDefaultAsync(i => i.ItemId == id && !i.IsDeleted);

        if (item is null)
        {
            return NotFound(new { message = $"Collection item {id} was not found." });
        }

        if (item.UserName != input.UserName)
        {
            return BadRequest(new { message = "You can only edit items in your own collection." });
        }

        var validationError = await ValidateAsync(input);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        item.ItemName = input.ItemName.Trim();
        item.StartingQuantity = input.StartingQuantity;
        item.CurrentQuantity = input.CurrentQuantity;
        item.LastUpdatedAt = DateTime.UtcNow;

     
        item.CollectionItemCategories.Clear();
        foreach (var code in input.CategoryCodes.Distinct())
        {
            item.CollectionItemCategories.Add(new CollectionItemCategory { ItemId = item.ItemId, CategoryCode = code });
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated collection item {ItemId}", id);

        return NoContent();
    }

    // DELETE api/collectionitems/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.CollectionItems.FirstOrDefaultAsync(i => i.ItemId == id && !i.IsDeleted);
        if (item is null)
        {
            return NotFound(new { message = $"Collection item {id} was not found." });
        }


        item.IsDeleted = true;
        item.LastUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Soft-deleted collection item {ItemId}", id);

        return NoContent();
    }

    private async Task<string?> ValidateAsync(CollectionItemInputDto input)
    {
        if (input.CurrentQuantity > input.StartingQuantity)
        {
            return "Current quantity cannot be greater than starting quantity.";
        }

        if (!await _db.ArcaneVaultUsers.AnyAsync(u => u.UserName == input.UserName && !u.IsDeleted))
        {
            return $"Account '{input.UserName}' does not exist.";
        }

        var codes = input.CategoryCodes.Distinct().ToList();
        if (codes.Count > 0)
        {
            var existingCount = await _db.Categories.CountAsync(c => codes.Contains(c.CategoryCode));
            if (existingCount != codes.Count)
            {
                return "One or more selected categories no longer exist.";
            }
        }

        return null;
    }
}

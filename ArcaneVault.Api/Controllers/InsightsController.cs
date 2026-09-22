// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508              
using ArcaneVault.Api.Data;
using ArcaneVault.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//Calculations 
//This is also where the API queries Entity Framework Core, crunches the
//StartingQuantity and the CurrentQuantity gap, it applies to my 50/30/20 weights and spits out the final 0-100 score


namespace ArcaneVault.Api.Controllers;


[ApiController]
[Route("api/insights")]
public class InsightsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<InsightsController> _logger;

    
    private const decimal TurnoverWeight = 0.50m;
    private const decimal ReachWeight = 0.30m;
    private const decimal ScarcityWeight = 0.20m;

    public InsightsController(AppDbContext db, ILogger<InsightsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    
    private sealed record RawSignal(
        string ItemName,
        int CollectorReach,
        int TotalStarting,
        int TotalCurrent,
        string[] CategoryNames);


    private async Task<List<RawSignal>> LoadRawSignalsAsync()
    {
       
        var rows = await _db.CollectionItems
            .Where(i => !i.IsDeleted)
            .Select(i => new
            {
                i.ItemName,
                i.UserName,
                i.StartingQuantity,
                i.CurrentQuantity,
            })
            .ToListAsync();

       
        var aggregates = rows
            .GroupBy(r => r.ItemName)
            .Select(g => new
            {
                ItemName = g.Key,
                CollectorReach = g.Select(r => r.UserName).Distinct().Count(),
                TotalStarting = g.Sum(r => r.StartingQuantity),
                TotalCurrent = g.Sum(r => r.CurrentQuantity),
            })
            .ToList();

        
        var categoryRows = await _db.CollectionItemCategories
            .Where(cic => !cic.CollectionItem.IsDeleted)
            .Select(cic => new
            {
                cic.CollectionItem.ItemName,
                cic.Category.CategoryName,
            })
            .ToListAsync();

  
        var categoryLookup = categoryRows
            .GroupBy(x => x.ItemName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.CategoryName).Distinct().OrderBy(n => n).ToArray());

        return aggregates
            .Select(a => new RawSignal(
                a.ItemName,
                a.CollectorReach,
                a.TotalStarting,
                a.TotalCurrent,
                categoryLookup.GetValueOrDefault(a.ItemName, [])))
            .ToList();
    }

   
    private static List<DemandSignalDto> ScoreSignals(List<RawSignal> raw)
    {
        if (raw.Count == 0)
        {
            return [];
        }

        
        var maxReach = raw.Max(r => r.CollectorReach);
        var maxRemaining = raw.Max(r => r.TotalCurrent);

        var scored = raw.Select(r =>
        {
            var unitsMoved = r.TotalStarting - r.TotalCurrent;

            
            var turnoverRate = r.TotalStarting == 0
                ? 0m
                : Math.Round((decimal)unitsMoved / r.TotalStarting * 100m, 1);

            
            var reachScore = maxReach == 0
                ? 0m
                : Math.Round((decimal)r.CollectorReach / maxReach * 100m, 1);

            
            var scarcityScore = maxRemaining == 0
                ? 100m
                : Math.Round((1m - (decimal)r.TotalCurrent / maxRemaining) * 100m, 1);

            var demandIndex = Math.Round(
                turnoverRate * TurnoverWeight
                + reachScore * ReachWeight
                + scarcityScore * ScarcityWeight, 1);

            return new DemandSignalDto
            {
                ItemName = r.ItemName,
                CategoryNames = r.CategoryNames,
                CollectorReach = r.CollectorReach,
                TotalStartingQuantity = r.TotalStarting,
                TotalCurrentQuantity = r.TotalCurrent,
                UnitsMoved = unitsMoved,
                TurnoverRate = turnoverRate,
                ReachScore = reachScore,
                ScarcityScore = scarcityScore,
                DemandIndex = demandIndex,
                Tier = ClassifyTier(demandIndex),
            };
        });

        return scored.OrderByDescending(s => s.DemandIndex).ToList();
    }

    private static string ClassifyTier(decimal demandIndex) => demandIndex switch
    {
        >= 60m => "Hot",
        >= 40m => "Rising",
        >= 20m => "Steady",
        _ => "Dormant",
    };

   
    [HttpGet("overview")]
    public async Task<ActionResult<VaultOverviewDto>> GetOverview() => Ok(await ComputeOverviewAsync());

   
    private async Task<VaultOverviewDto> ComputeOverviewAsync()
    {
        var activeSince = DateTime.UtcNow.AddDays(-30);

        var totalCollectors = await _db.ArcaneVaultUsers
            .CountAsync(u => !u.IsDeleted && u.RoleId == RoleIds.User);

        var activeCollectors = await _db.CollectionItems
            .Where(i => !i.IsDeleted && i.LastUpdatedAt >= activeSince)
            .Select(i => i.UserName)
            .Distinct()
            .CountAsync();

        var items = await _db.CollectionItems
            .Where(i => !i.IsDeleted)
            .Select(i => new { i.ItemName, i.StartingQuantity, i.CurrentQuantity })
            .ToListAsync();

        var totalStarting = items.Sum(i => i.StartingQuantity);
        var totalCurrent = items.Sum(i => i.CurrentQuantity);
        var totalMoved = totalStarting - totalCurrent;

        return new VaultOverviewDto
        {
            TotalCollectors = totalCollectors,
            ActiveCollectors = activeCollectors,
            TotalItemRecords = items.Count,
            DistinctCollectibles = items.Select(i => i.ItemName).Distinct().Count(),
            TotalUnitsHeld = totalCurrent,
            TotalUnitsMoved = totalMoved,
            OverallTurnoverRate = totalStarting == 0
                ? 0m
                : Math.Round((decimal)totalMoved / totalStarting * 100m, 1),
        };
    }

    [HttpGet("demand")]
    public async Task<ActionResult<List<DemandSignalDto>>> GetDemandSignals([FromQuery] int top = 10)
    {
        top = top is < 1 or > 100 ? 10 : top;

        var signals = ScoreSignals(await LoadRawSignalsAsync());
        return Ok(signals.Take(top).ToList());
    }

   
    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryPopularityDto>>> GetCategoryPopularity() =>
        Ok(await ComputeCategoryPopularityAsync());

    private async Task<List<CategoryPopularityDto>> ComputeCategoryPopularityAsync()
    {
  
        var rows = await _db.CollectionItemCategories
            .Where(cic => !cic.CollectionItem.IsDeleted)
            .Select(cic => new
            {
                cic.CategoryCode,
                cic.Category.CategoryName,
                cic.CollectionItem.UserName,
                UnitsMoved = cic.CollectionItem.StartingQuantity - cic.CollectionItem.CurrentQuantity,
            })
            .ToListAsync();

     
        var grouped = rows
            .GroupBy(r => new { r.CategoryCode, r.CategoryName })
            .Select(g => new
            {
                g.Key.CategoryCode,
                g.Key.CategoryName,
                ItemCount = g.Count(),
                CollectorReach = g.Select(r => r.UserName).Distinct().Count(),
                UnitsMoved = g.Sum(r => r.UnitsMoved),
            })
            .ToList();

        var totalItems = grouped.Sum(g => g.ItemCount);

        var result = grouped
            .Select(g => new CategoryPopularityDto
            {
                CategoryCode = g.CategoryCode,
                CategoryName = g.CategoryName,
                ItemCount = g.ItemCount,
                CollectorReach = g.CollectorReach,
                UnitsMoved = g.UnitsMoved,
                ShareOfItems = totalItems == 0 ? 0m : Math.Round((decimal)g.ItemCount / totalItems * 100m, 1),
            })
            .OrderByDescending(c => c.ItemCount)
            .ToList();

        return result;
    }

    
    [HttpGet("activity")]
    public async Task<ActionResult<List<ActivityPointDto>>> GetActivityTrend([FromQuery] int months = 6)
    {
        months = months is < 1 or > 24 ? 6 : months;

        var today = DateTime.Today;
        var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-(months - 1));

        var items = await _db.CollectionItems
            .Where(i => !i.IsDeleted && (i.CreatedAt >= firstMonth || i.LastUpdatedAt >= firstMonth))
            .Select(i => new { i.CreatedAt, i.LastUpdatedAt })
            .ToListAsync();

        var points = new List<ActivityPointDto>();

        
        for (var cursor = firstMonth; cursor <= today; cursor = cursor.AddMonths(1))
        {
            var monthStart = cursor;
            var monthEnd = cursor.AddMonths(1);

            points.Add(new ActivityPointDto
            {
                Label = cursor.ToString("MMM yyyy"),
                ItemsAdded = items.Count(i => i.CreatedAt >= monthStart && i.CreatedAt < monthEnd),
                
                ItemsUpdated = items.Count(i => i.LastUpdatedAt >= monthStart
                                             && i.LastUpdatedAt < monthEnd
                                             && i.LastUpdatedAt.Date != i.CreatedAt.Date),
            });
        }

        return Ok(points);
    }

    [HttpGet("highlights")]
    public async Task<ActionResult<List<InsightHighlightDto>>> GetHighlights()
    {
        var highlights = new List<InsightHighlightDto>();

        var signals = ScoreSignals(await LoadRawSignalsAsync());
        if (signals.Count == 0)
        {
            highlights.Add(new InsightHighlightDto
            {
                Message = "No collection data yet. Once collectors start adding items, demand insights will appear here.",
                Severity = "info",
            });
            return Ok(highlights);
        }

        var hottest = signals[0];
        highlights.Add(new InsightHighlightDto
        {
            Message = $"\"{hottest.ItemName}\" has the strongest demand signal at {hottest.DemandIndex:N1}/100 - held by {hottest.CollectorReach} collector(s), with {hottest.TurnoverRate:N1}% of acquired units already moved on.",
            Severity = hottest.DemandIndex >= 60m ? "danger" : "info",
        });

        var hotCount = signals.Count(s => s.Tier == "Hot");
        if (hotCount > 0)
        {
            highlights.Add(new InsightHighlightDto
            {
                Message = $"{hotCount} of {signals.Count} collectibles are in the Hot tier, suggesting genuine trading appetite worth targeting in the marketplace build.",
                Severity = "warning",
            });
        }

        var dormant = signals.Where(s => s.Tier == "Dormant").ToList();
        if (dormant.Count > 0)
        {
            highlights.Add(new InsightHighlightDto
            {
                Message = $"{dormant.Count} collectible(s) show almost no movement, led by \"{dormant[0].ItemName}\". These are candidates for promotion or bundling.",
                Severity = "info",
            });
        }

        var topCategory = (await ComputeCategoryPopularityAsync()).FirstOrDefault();
        if (topCategory is not null)
        {
            highlights.Add(new InsightHighlightDto
            {
                Message = $"{topCategory.CategoryName} is the most collected category, holding {topCategory.ShareOfItems:N1}% of all classified items across {topCategory.CollectorReach} collector(s).",
                Severity = "success",
            });
        }

        var overview = await ComputeOverviewAsync();
        if (overview.TotalCollectors > 0)
        {
            var engagement = Math.Round((decimal)overview.ActiveCollectors / overview.TotalCollectors * 100m, 1);
            highlights.Add(new InsightHighlightDto
            {
                Message = $"{overview.ActiveCollectors} of {overview.TotalCollectors} collectors ({engagement:N1}%) updated their collection in the last 30 days.",
                Severity = engagement >= 50m ? "success" : "warning",
            });
        }

        return Ok(highlights);
    }
}

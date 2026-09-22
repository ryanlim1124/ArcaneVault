// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
namespace ArcaneVault.Web.Models;

/// <summary>Mirrors ArcaneVault.Api.Dtos.VaultOverviewDto.</summary>
public class VaultOverviewDto
{
    public int TotalCollectors { get; set; }
    public int ActiveCollectors { get; set; }
    public int TotalItemRecords { get; set; }
    public int DistinctCollectibles { get; set; }
    public int TotalUnitsHeld { get; set; }
    public int TotalUnitsMoved { get; set; }
    public decimal OverallTurnoverRate { get; set; }
}

/// <summary>Mirrors ArcaneVault.Api.Dtos.DemandSignalDto.</summary>
public class DemandSignalDto
{
    public string ItemName { get; set; } = string.Empty;
    public string[] CategoryNames { get; set; } = [];
    public int CollectorReach { get; set; }
    public int TotalStartingQuantity { get; set; }
    public int TotalCurrentQuantity { get; set; }
    public int UnitsMoved { get; set; }
    public decimal TurnoverRate { get; set; }
    public decimal ReachScore { get; set; }
    public decimal ScarcityScore { get; set; }
    public decimal DemandIndex { get; set; }
    public string Tier { get; set; } = "Dormant";
}

/// <summary>Mirrors ArcaneVault.Api.Dtos.CategoryPopularityDto.</summary>
public class CategoryPopularityDto
{
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int CollectorReach { get; set; }
    public int UnitsMoved { get; set; }
    public decimal ShareOfItems { get; set; }
}

/// <summary>Mirrors ArcaneVault.Api.Dtos.ActivityPointDto.</summary>
public class ActivityPointDto
{
    public string Label { get; set; } = string.Empty;
    public int ItemsAdded { get; set; }
    public int ItemsUpdated { get; set; }
}

/// <summary>Mirrors ArcaneVault.Api.Dtos.InsightHighlightDto.</summary>
public class InsightHighlightDto
{
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
}

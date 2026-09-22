// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
namespace ArcaneVault.Api.Dtos;

/// <summary>Platform-wide headline figures for the top of the Vault Insights dashboard.</summary>
public class VaultOverviewDto
{
    public int TotalCollectors { get; set; }

    /// <summary>Collectors who added or edited an item within the last 30 days.</summary>
    public int ActiveCollectors { get; set; }

    /// <summary>Number of collection-item records across all users.</summary>
    public int TotalItemRecords { get; set; }

    /// <summary>Number of distinct collectibles by name (what the demand ranking operates on).</summary>
    public int DistinctCollectibles { get; set; }

    public int TotalUnitsHeld { get; set; }
    public int TotalUnitsMoved { get; set; }

    /// <summary>Share of all acquired units that have since left collections, as a percentage.</summary>
    public decimal OverallTurnoverRate { get; set; }
}

/// <summary>
/// One collectible's demand profile - the core output of the Vault Insights feature.
/// Every component score is exposed alongside the final index so the number is auditable
/// rather than a black box.
/// </summary>
public class DemandSignalDto
{
    public string ItemName { get; set; } = string.Empty;
    public string[] CategoryNames { get; set; } = [];

    /// <summary>How many distinct collectors hold this collectible.</summary>
    public int CollectorReach { get; set; }

    public int TotalStartingQuantity { get; set; }
    public int TotalCurrentQuantity { get; set; }

    /// <summary>Units that have left collections (starting - current).</summary>
    public int UnitsMoved { get; set; }

    /// <summary>Component 1 (weight 0.50): share of acquired units that have moved on, 0-100.</summary>
    public decimal TurnoverRate { get; set; }

    /// <summary>Component 2 (weight 0.30): collector reach normalised against the most widely held item, 0-100.</summary>
    public decimal ReachScore { get; set; }

    /// <summary>Component 3 (weight 0.20): how little remains in circulation relative to the largest holding, 0-100.</summary>
    public decimal ScarcityScore { get; set; }

    /// <summary>The weighted composite score, 0-100.</summary>
    public decimal DemandIndex { get; set; }

    /// <summary>Banding of <see cref="DemandIndex"/> for display: Hot / Rising / Steady / Dormant.</summary>
    public string Tier { get; set; } = "Dormant";
}

/// <summary>How much of the platform's activity sits in one category.</summary>
public class CategoryPopularityDto
{
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Collection-item records classified under this category.</summary>
    public int ItemCount { get; set; }

    /// <summary>Distinct collectors holding at least one item in this category.</summary>
    public int CollectorReach { get; set; }

    public int UnitsMoved { get; set; }

    /// <summary>This category's share of all classified items, as a percentage.</summary>
    public decimal ShareOfItems { get; set; }
}

/// <summary>One month on the platform-activity trend chart.</summary>
public class ActivityPointDto
{
    public string Label { get; set; } = string.Empty;
    public int ItemsAdded { get; set; }
    public int ItemsUpdated { get; set; }
}

/// <summary>A rule-based, human-readable observation shown in the Insights panel.</summary>
public class InsightHighlightDto
{
    public string Message { get; set; } = string.Empty;

    /// <summary>"info" | "success" | "warning" | "danger" - maps to a Bootstrap alert class.</summary>
    public string Severity { get; set; } = "info";
}

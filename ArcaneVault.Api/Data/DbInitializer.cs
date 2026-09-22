// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace ArcaneVault.Api.Data;

/// <summary>
/// Populates a freshly-created database with demo data so the prototype is immediately
/// usable and the Vault Insights dashboard always has something meaningful to show.
///
/// Two deliberate design choices:
///  1. All dates are generated RELATIVE to DateTime.Today, never hard-coded, so the
///     activity charts look current whenever the project is run.
///  2. Item quantities are generated from a "demand tier" per collectible, so the
///     Demand Index leaderboard reliably shows a clear spread from hot to cold.
/// </summary>
public static class DbInitializer
{
    /// <summary>Fixed seed so the generated demo data is identical between runs.</summary>
    private static readonly Random Rng = new(252206);

    /// <summary>Shared password for every seeded demo account (documented in the README).</summary>
    private const string StaffPassword = "Staff@123";
    private const string UserPassword = "User@123";

    public static void Seed(AppDbContext db, IPasswordHasher<ArcaneVaultUser> hasher)
    {
        // Only ever seed an empty database - never overwrite real data.
        if (db.ArcaneVaultUserRoles.Any())
        {
            return;
        }

        SeedRoles(db);
        var users = SeedUsers(db, hasher);
        var categories = SeedCategories(db);
        SeedCollectionItems(db, users, categories);
    }

    // ---------------------------------------------------------------
    // Roles
    // ---------------------------------------------------------------
    private static void SeedRoles(AppDbContext db)
    {
        db.ArcaneVaultUserRoles.AddRange(
            new ArcaneVaultUserRole { RoleId = RoleIds.User, RoleName = "User" },
            new ArcaneVaultUserRole { RoleId = RoleIds.Staff, RoleName = "Staff" });

        db.SaveChanges();
    }

    // ---------------------------------------------------------------
    // Accounts
    // ---------------------------------------------------------------
    private static List<ArcaneVaultUser> SeedUsers(AppDbContext db, IPasswordHasher<ArcaneVaultUser> hasher)
    {
        var users = new List<ArcaneVaultUser>
        {
            // Administrator account used to demonstrate Category Management and Vault Insights.
            NewUser("staff", "staff@arcanevault.sg", RoleIds.Staff, StaffPassword, hasher),

            // Primary demo collector account.
            NewUser("collector", "collector@arcanevault.sg", RoleIds.User, UserPassword, hasher),

            // Additional collectors, so platform-wide analytics have several holders per item.
            NewUser("cardmaster_lee", "lee@example.sg", RoleIds.User, UserPassword, hasher),
            NewUser("vault_amelia", "amelia@example.sg", RoleIds.User, UserPassword, hasher),
            NewUser("retro_toys_sg", "retro@example.sg", RoleIds.User, UserPassword, hasher),
            NewUser("kaiju_ken", "ken@example.sg", RoleIds.User, UserPassword, hasher),
            NewUser("mintcondition", "mint@example.sg", RoleIds.User, UserPassword, hasher),
            NewUser("figure_hunter", "hunter@example.sg", RoleIds.User, UserPassword, hasher),
            NewUser("tcg_tanya", "tanya@example.sg", RoleIds.User, UserPassword, hasher),
        };

        db.ArcaneVaultUsers.AddRange(users);
        db.SaveChanges();

        return users;
    }

    private static ArcaneVaultUser NewUser(
        string userName, string email, int roleId, string password, IPasswordHasher<ArcaneVaultUser> hasher)
    {
        var user = new ArcaneVaultUser
        {
            UserName = userName,
            Email = email,
            RoleId = roleId,
            IsDeleted = false,
        };

        // Hash with PBKDF2 - the plain-text password is never persisted.
        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }

    // ---------------------------------------------------------------
    // Categories (platform master data, maintained by Staff)
    // ---------------------------------------------------------------
    private static Dictionary<string, Category> SeedCategories(AppDbContext db)
    {
        var categories = new List<Category>
        {
            new() { CategoryCode = "TCG",   CategoryName = "Trading Card Games" },
            new() { CategoryCode = "FIG",   CategoryName = "Figurines & Statues" },
            new() { CategoryCode = "COMIC", CategoryName = "Comics & Manga" },
            new() { CategoryCode = "COIN",  CategoryName = "Coins & Currency" },
            new() { CategoryCode = "STAMP", CategoryName = "Stamps" },
            new() { CategoryCode = "MODEL", CategoryName = "Model Kits" },
            new() { CategoryCode = "PLUSH", CategoryName = "Plush Toys" },
            new() { CategoryCode = "VINYL", CategoryName = "Vinyl Records" },
            new() { CategoryCode = "RETRO", CategoryName = "Retro Video Games" },
            new() { CategoryCode = "SPORT", CategoryName = "Sports Memorabilia" },
        };

        db.Categories.AddRange(categories);
        db.SaveChanges();

        return categories.ToDictionary(c => c.CategoryCode);
    }

    // ---------------------------------------------------------------
    // Collection items
    // ---------------------------------------------------------------

    /// <summary>
    /// How much demand a collectible is meant to demonstrate. The tier controls how many
    /// collectors hold it and what proportion of their original quantity they still keep -
    /// which is precisely what the Demand Index measures.
    /// </summary>
    private enum DemandTier
    {
        Hot,     // widely held and moving fast out of collections
        Warm,
        Medium,
        Cold,    // barely traded, few holders
    }

    /// <summary>One row of the demo collectible catalogue.</summary>
    private record CatalogueEntry(string Name, string[] CategoryCodes, DemandTier Tier);

    private static readonly CatalogueEntry[] Catalogue =
    [
        // --- Hot: high turnover, many holders ---
        new("Charizard Base Set Holo",            ["TCG"],            DemandTier.Hot),
        new("Gundam RX-78-2 Perfect Grade",       ["MODEL", "FIG"],   DemandTier.Hot),
        new("Pokemon Red Cartridge (Sealed)",     ["RETRO"],          DemandTier.Hot),
        new("One Piece Luffy Gear 5 Figure",      ["FIG"],            DemandTier.Hot),

        // --- Warm ---
        new("Pikachu Illustrator Promo",          ["TCG"],            DemandTier.Warm),
        new("Spider-Man #1 (1990)",               ["COMIC"],          DemandTier.Warm),
        new("Dragon Ball Z Shenron Statue",       ["FIG"],            DemandTier.Warm),
        new("Singapore 1967 Orchid Series $1",    ["COIN"],           DemandTier.Warm),
        new("Yu-Gi-Oh Blue-Eyes 1st Edition",     ["TCG"],            DemandTier.Warm),
        new("Michael Jordan Rookie Card",         ["SPORT", "TCG"],   DemandTier.Warm),

        // --- Medium ---
        new("Marvel Legends Iron Man MK85",       ["FIG"],            DemandTier.Medium),
        new("Astro Boy Vintage Tin Robot",        ["RETRO", "FIG"],   DemandTier.Medium),
        new("Beatles Abbey Road First Press",     ["VINYL"],          DemandTier.Medium),
        new("Hello Kitty 50th Anniversary Plush", ["PLUSH"],          DemandTier.Medium),
        new("Evangelion Unit-01 Model Kit",       ["MODEL"],          DemandTier.Medium),
        new("Naruto Shippuden Manga Vol.1",       ["COMIC"],          DemandTier.Medium),

        // --- Cold: rarely moves, few holders ---
        new("Penny Black 1840 Replica",           ["STAMP"],          DemandTier.Cold),
        new("Star Wars Millennium Falcon LEGO",   ["MODEL"],          DemandTier.Cold),
        new("Tamagotchi P1 Original",             ["RETRO"],          DemandTier.Cold),
        new("SG 50th Anniversary Coin Set",       ["COIN"],           DemandTier.Cold),
        new("Studio Ghibli Totoro Plush",         ["PLUSH"],          DemandTier.Cold),
        new("Pink Floyd Dark Side Vinyl",         ["VINYL"],          DemandTier.Cold),
        new("Straits Settlements 1903 Dollar",    ["COIN", "STAMP"],  DemandTier.Cold),
        new("Ultraman Showa Era Soft Vinyl",      ["FIG", "RETRO"],   DemandTier.Cold),
    ];

    /// <summary>Holder-count range and retention range (how much of the original quantity is kept) per tier.</summary>
    private static (int MinHolders, int MaxHolders, double MinRetention, double MaxRetention) TierProfile(DemandTier tier) => tier switch
    {
        DemandTier.Hot    => (6, 8, 0.15, 0.35),
        DemandTier.Warm   => (4, 6, 0.40, 0.60),
        DemandTier.Medium => (2, 4, 0.65, 0.85),
        _                 => (1, 2, 0.92, 1.00),
    };

    private static void SeedCollectionItems(AppDbContext db, List<ArcaneVaultUser> users, Dictionary<string, Category> categories)
    {
        // Only collector accounts hold collections in the demo data.
        var collectors = users.Where(u => u.RoleId == RoleIds.User).ToList();
        var today = DateTime.Today;
        var items = new List<CollectionItem>();

        foreach (var entry in Catalogue)
        {
            var (minHolders, maxHolders, minRetention, maxRetention) = TierProfile(entry.Tier);
            var holderCount = Math.Min(Rng.Next(minHolders, maxHolders + 1), collectors.Count);

            // Pick a distinct random set of collectors to hold this collectible.
            foreach (var owner in collectors.OrderBy(_ => Rng.Next()).Take(holderCount))
            {
                var startingQuantity = Rng.Next(2, 9);

                // Retention is the share of the original quantity still held. The remainder
                // has left the collection - that difference is the demand signal.
                var retention = minRetention + Rng.NextDouble() * (maxRetention - minRetention);
                var currentQuantity = (int)Math.Round(startingQuantity * retention);
                currentQuantity = Math.Clamp(currentQuantity, 0, startingQuantity);

                // Added between 6 months ago and today, skewed towards recent months so the
                // activity trend chart shows the platform growing.
                var daysAgo = (int)Math.Round(Math.Pow(Rng.NextDouble(), 1.6) * 180);
                var createdAt = today.AddDays(-daysAgo);

                var item = new CollectionItem
                {
                    ItemName = entry.Name,
                    UserName = owner.UserName,
                    User = owner,
                    StartingQuantity = startingQuantity,
                    CurrentQuantity = currentQuantity,
                    IsDeleted = false,
                    CreatedAt = createdAt,
                    // Items that have moved were edited at some point between being added and
                    // today. Rng.Next(0, daysAgo + 1) yields 0..daysAgo inclusive, so the result
                    // can never land past today - important, because a "last updated" date in
                    // the future is both visibly wrong on the details page and would fall
                    // outside the activity chart's month window.
                    LastUpdatedAt = currentQuantity < startingQuantity
                        ? createdAt.AddDays(Rng.Next(0, daysAgo + 1))
                        : createdAt,
                };

                foreach (var code in entry.CategoryCodes)
                {
                    item.CollectionItemCategories.Add(new CollectionItemCategory
                    {
                        CategoryCode = code,
                        Category = categories[code],
                    });
                }

                items.Add(item);
            }
        }

        db.CollectionItems.AddRange(items);
        db.SaveChanges();
    }
}

/// <summary>Fixed role identifiers, referenced by the seeder and the authorisation checks.</summary>
public static class RoleIds
{
    public const int User = 1;
    public const int Staff = 2;
}

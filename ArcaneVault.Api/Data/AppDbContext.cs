// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Api.Models;
using Microsoft.EntityFrameworkCore;

//Bridge between c# and the SQLite Database arcanevault.db, 
//tells entity framework core what tables exist and how they relate to each other

namespace ArcaneVault.Api.Data;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ArcaneVaultUser> ArcaneVaultUsers => Set<ArcaneVaultUser>();
    public DbSet<ArcaneVaultUserRole> ArcaneVaultUserRoles => Set<ArcaneVaultUserRole>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CollectionItem> CollectionItems => Set<CollectionItem>();
    public DbSet<CollectionItemCategory> CollectionItemCategories => Set<CollectionItemCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------------------------------------------------------
        // ArcaneVaultUserRoles
        // ---------------------------------------------------------------
        modelBuilder.Entity<ArcaneVaultUserRole>(entity =>
        {
            entity.ToTable("ArcaneVaultUserRoles");
            entity.HasKey(r => r.RoleId);


            entity.Property(r => r.RoleId).ValueGeneratedNever();

            entity.HasIndex(r => r.RoleName).IsUnique();
        });

        // ---------------------------------------------------------------
        // ArcaneVaultUsers
        // ---------------------------------------------------------------
        modelBuilder.Entity<ArcaneVaultUser>(entity =>
        {
            entity.ToTable("ArcaneVaultUsers");


            entity.HasKey(u => u.UserName);

           
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------------------------------------------------------
        // Categories
        // ---------------------------------------------------------------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            // Natural string primary key (the category code), per the assignment schema.
            entity.HasKey(c => c.CategoryCode);

            entity.HasIndex(c => c.CategoryName).IsUnique();
        });

        // ---------------------------------------------------------------
        // CollectionItems
        // ---------------------------------------------------------------
        modelBuilder.Entity<CollectionItem>(entity =>
        {
            entity.ToTable("CollectionItems");
            entity.HasKey(i => i.ItemId);


            entity.HasOne(i => i.User)
                  .WithMany(u => u.CollectionItems)
                  .HasForeignKey(i => i.UserName)
                  .OnDelete(DeleteBehavior.Restrict);
        });

       
        modelBuilder.Entity<CollectionItem>()
            .HasMany(i => i.Categories)
            .WithMany(c => c.CollectionItems)
            .UsingEntity<CollectionItemCategory>(

                right => right
                    .HasOne(cic => cic.Category)
                    .WithMany(c => c.CollectionItemCategories)
                    .HasForeignKey(cic => cic.CategoryCode)
                    
                    .OnDelete(DeleteBehavior.Restrict),
                left => left
                    .HasOne(cic => cic.CollectionItem)
                    .WithMany(i => i.CollectionItemCategories)
                    .HasForeignKey(cic => cic.ItemId)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("CollectionItemCategories");
                    join.HasKey(cic => new { cic.ItemId, cic.CategoryCode });
                });
    }
}

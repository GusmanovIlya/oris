using EfCoreHomework.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCoreHomework.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>()
            .HasMany(category => category.Products)
            .WithOne(product => product.Category)
            .HasForeignKey(product => product.CategoryId);

        modelBuilder.Entity<Category>().HasData(DataSeed.Categories);
        modelBuilder.Entity<Product>().HasData(DataSeed.Products);
    }
}

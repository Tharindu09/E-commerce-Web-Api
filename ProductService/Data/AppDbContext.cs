using Microsoft.EntityFrameworkCore;
using ProductService.Model;

namespace ProductService.Data;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Inventory> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
        .HasOne(p => p.Inventory)
        .WithOne(i => i.Product)
        .HasForeignKey<Inventory>(i => i.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
    }
    


}

using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

// AppDbContext is database session.
// EF Core uses this class to read/write tables.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<User> Users { get; set; }
    public DbSet<Address> Address { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id); // Primary Key
            entity.Property(u => u.Id).UseIdentityAlwaysColumn(); // Auto-increment
            entity.HasIndex(u => u.Email).IsUnique();

            // One-to-one relationship
            entity.HasOne(u => u.address)    // User has one Address
                  .WithOne(a => a.User)      // Address has one User
                  .HasForeignKey<Address>(a => a.UserId) // FK is in Address
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }



}

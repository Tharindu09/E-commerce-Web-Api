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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id); //PK
            entity.Property(u => u.Id).UseIdentityAlwaysColumn(); //ID auto generate

            entity.HasIndex(u => u.Email).IsUnique();



        }
        );
    }



}

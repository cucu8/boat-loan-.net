using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.Entities;

public class BoatRentalDbContext : DbContext
{
    public BoatRentalDbContext(DbContextOptions<BoatRentalDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Boat> Boats { get; set; }

    public DbSet<BoatImage> BoatImages { get; set; }

    public DbSet<City> Cities { get; set; }

    public DbSet<Country> Countries { get; set; }

    public DbSet<District> Districts { get; set; }

    public DbSet<UserCredential> UserCredentials { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is User || e.Entity is Boat)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Boats)
            .WithOne(b => b.Owner)
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Boat>()
            .HasMany(b => b.Images)
            .WithOne(i => i.Boat)
            .HasForeignKey(i => i.BoatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

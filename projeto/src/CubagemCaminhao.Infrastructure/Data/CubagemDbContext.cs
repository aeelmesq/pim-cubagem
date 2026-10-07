using CubagemCaminhao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CubagemCaminhao.Infrastructure.Data;

public sealed class CubagemDbContext(DbContextOptions<CubagemDbContext> options) : DbContext(options)
{
    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<CargoItem> CargoItems => Set<CargoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Truck>(entity =>
        {
            entity.Property(truck => truck.Name).HasMaxLength(120).IsRequired();
            entity.Property(truck => truck.Plate).HasMaxLength(10).IsRequired();
            entity.Property(truck => truck.LengthCm).HasPrecision(10, 2);
            entity.Property(truck => truck.WidthCm).HasPrecision(10, 2);
            entity.Property(truck => truck.HeightCm).HasPrecision(10, 2);
            entity.HasIndex(truck => truck.Plate).IsUnique();
        });

        modelBuilder.Entity<CargoItem>(entity =>
        {
            entity.Property(cargo => cargo.Name).HasMaxLength(120).IsRequired();
            entity.Property(cargo => cargo.LengthCm).HasPrecision(10, 2);
            entity.Property(cargo => cargo.WidthCm).HasPrecision(10, 2);
            entity.Property(cargo => cargo.HeightCm).HasPrecision(10, 2);
            entity.HasOne(cargo => cargo.Truck)
                .WithMany(truck => truck.CargoItems)
                .HasForeignKey(cargo => cargo.TruckId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

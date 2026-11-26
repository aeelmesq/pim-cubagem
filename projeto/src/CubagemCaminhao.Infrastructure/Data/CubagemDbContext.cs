using CubagemCaminhao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CubagemCaminhao.Infrastructure.Data;

public sealed class CubagemDbContext(DbContextOptions<CubagemDbContext> options) : DbContext(options)
{
    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<CargoItem> CargoItems => Set<CargoItem>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Load> Loads => Set<Load>();
    public DbSet<LoadItem> LoadItems => Set<LoadItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Truck>(entity =>
        {
            entity.Property(truck => truck.Name).HasMaxLength(120).IsRequired();
            entity.Property(truck => truck.Plate).HasMaxLength(10).IsRequired();
            entity.Property(truck => truck.LengthCm).HasPrecision(10, 2);
            entity.Property(truck => truck.WidthCm).HasPrecision(10, 2);
            entity.Property(truck => truck.HeightCm).HasPrecision(10, 2);
            entity.Property(truck => truck.MaxLoadKg).HasPrecision(10, 2);
            entity.HasIndex(truck => truck.Plate).IsUnique();

            // Motorista é opcional. Cada motorista fica em um caminhão só.
            // Se o motorista for excluído, o caminhão fica sem motorista.
            entity.HasIndex(truck => truck.DriverId).IsUnique();
            entity.HasOne(truck => truck.Driver)
                .WithMany()
                .HasForeignKey(truck => truck.DriverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.Property(driver => driver.Name).HasMaxLength(120).IsRequired();
            entity.Property(driver => driver.Username).HasMaxLength(40).IsRequired();
            entity.Property(driver => driver.PasswordHash).HasMaxLength(200).IsRequired();
            entity.HasIndex(driver => driver.Username).IsUnique();
        });

        modelBuilder.Entity<Load>(entity =>
        {
            entity.Property(load => load.TruckName).HasMaxLength(120).IsRequired();
            entity.Property(load => load.TruckPlate).HasMaxLength(10).IsRequired();
            entity.Property(load => load.DriverName).HasMaxLength(120);
            entity.Property(load => load.Status).HasMaxLength(20).IsRequired();
            entity.Property(load => load.TruckVolumeM3).HasPrecision(12, 3);
            entity.Property(load => load.MaxLoadKg).HasPrecision(10, 2);
            entity.Property(load => load.CargoVolumeM3).HasPrecision(12, 3);
            entity.Property(load => load.OccupancyPercent).HasPrecision(8, 1);
            entity.Property(load => load.TotalWeightKg).HasPrecision(14, 2);
            entity.Property(load => load.CubedWeightKg).HasPrecision(14, 2);
            entity.Property(load => load.FreightWeightKg).HasPrecision(14, 2);
            entity.HasIndex(load => load.TruckId);
            entity.HasIndex(load => load.DriverId);
            entity.HasMany(load => load.Items)
                .WithOne()
                .HasForeignKey(item => item.LoadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoadItem>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(120).IsRequired();
            entity.Property(item => item.LengthCm).HasPrecision(10, 2);
            entity.Property(item => item.WidthCm).HasPrecision(10, 2);
            entity.Property(item => item.HeightCm).HasPrecision(10, 2);
            entity.Property(item => item.WeightKg).HasPrecision(10, 3);
        });

        modelBuilder.Entity<CargoItem>(entity =>
        {
            entity.Property(cargo => cargo.Name).HasMaxLength(120).IsRequired();
            entity.Property(cargo => cargo.LengthCm).HasPrecision(10, 2);
            entity.Property(cargo => cargo.WidthCm).HasPrecision(10, 2);
            entity.Property(cargo => cargo.HeightCm).HasPrecision(10, 2);
            entity.Property(cargo => cargo.WeightKg).HasPrecision(10, 3);
            entity.HasOne(cargo => cargo.Truck)
                .WithMany(truck => truck.CargoItems)
                .HasForeignKey(cargo => cargo.TruckId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

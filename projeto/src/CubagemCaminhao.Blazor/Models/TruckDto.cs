namespace CubagemCaminhao.Blazor.Models;

public sealed class TruckDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public decimal MaxLoadKg { get; set; }
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
    public decimal TruckVolumeM3 { get; set; }
    public decimal UsableVolumeM3 { get; set; }
    public decimal CargoVolumeM3 { get; set; }
    public decimal OccupancyPercent { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal CubedWeightKg { get; set; }
    public decimal FreightWeightKg { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<CargoDto> CargoItems { get; set; } = [];
}

public sealed class CargoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public int Quantity { get; set; }
    public decimal TotalVolumeM3 { get; set; }
    public decimal TotalWeightKg { get; set; }
}

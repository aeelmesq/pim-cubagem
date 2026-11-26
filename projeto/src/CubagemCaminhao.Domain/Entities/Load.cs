namespace CubagemCaminhao.Domain.Entities;

// Carregamento que saiu: uma "foto" da carga do caminhão no momento da saída.
// Guarda cópia dos dados (placa, motorista, itens e resultados do cálculo) para o histórico
// continuar igual mesmo se o caminhão ou o motorista forem alterados ou excluídos depois.
// Por isso TruckId e DriverId não são chaves estrangeiras: servem só para filtrar.
public sealed class Load
{
    public int Id { get; set; }
    public DateTimeOffset DispatchedAt { get; set; }

    public int TruckId { get; set; }
    public required string TruckName { get; set; }
    public required string TruckPlate { get; set; }
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }

    public decimal TruckVolumeM3 { get; set; }
    public decimal MaxLoadKg { get; set; }
    public decimal CargoVolumeM3 { get; set; }
    public decimal OccupancyPercent { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal CubedWeightKg { get; set; }
    public decimal FreightWeightKg { get; set; }
    public required string Status { get; set; }

    public ICollection<LoadItem> Items { get; } = new List<LoadItem>();
}

public sealed class LoadItem
{
    public int Id { get; set; }
    public int LoadId { get; set; }
    public required string Name { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public int Quantity { get; set; }
}

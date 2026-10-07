namespace CubagemCaminhao.Domain.Entities;

public sealed class Truck
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Plate { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public ICollection<CargoItem> CargoItems { get; } = new List<CargoItem>();
}

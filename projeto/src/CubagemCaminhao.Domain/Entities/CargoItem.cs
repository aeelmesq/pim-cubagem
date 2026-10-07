namespace CubagemCaminhao.Domain.Entities;

public sealed class CargoItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public int Quantity { get; set; }
    public int TruckId { get; set; }
    public Truck? Truck { get; set; }
}

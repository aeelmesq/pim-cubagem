namespace CubagemCaminhao.Blazor.Models;

public sealed class DriverDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Placa do caminhão do motorista, ou null se ele estiver sem caminhão.
    public string? TruckPlate { get; set; }
}

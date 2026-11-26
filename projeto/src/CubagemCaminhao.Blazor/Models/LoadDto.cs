namespace CubagemCaminhao.Blazor.Models;

// Uma carga do histórico (saída registrada de um caminhão).
public sealed class LoadDto
{
    // Horário de Brasília (UTC−3, sem horário de verão desde 2019).
    private static readonly TimeSpan BrasiliaOffset = TimeSpan.FromHours(-3);

    public int Id { get; set; }
    public DateTimeOffset DispatchedAt { get; set; }
    public int TruckId { get; set; }
    public string TruckName { get; set; } = string.Empty;
    public string TruckPlate { get; set; } = string.Empty;
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
    public decimal TruckVolumeM3 { get; set; }
    public decimal MaxLoadKg { get; set; }
    public decimal CargoVolumeM3 { get; set; }
    public decimal OccupancyPercent { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal CubedWeightKg { get; set; }
    public decimal FreightWeightKg { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<LoadItemDto> Items { get; set; } = [];

    public string DispatchedAtText => DispatchedAt.ToOffset(BrasiliaOffset).ToString("dd/MM/yyyy HH:mm");
}

public sealed class LoadItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public int Quantity { get; set; }
    public decimal TotalVolumeM3 { get; set; }
    public decimal TotalWeightKg { get; set; }
}

// Resposta de GET /api/me: o motorista logado, o caminhão dele e o histórico.
public sealed class MeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public TruckDto? Truck { get; set; }
    public List<LoadDto> Loads { get; set; } = [];
}

public sealed class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
}

// A situação vem pronta da API (OK, Atenção, Excede); aqui só escolhe a cor.
public static class StatusStyle
{
    public static string BadgeClass(string status) => status switch
    {
        "Excede" => "bg-danger",
        "Atenção" => "bg-warning text-dark",
        _ => "bg-success"
    };
}

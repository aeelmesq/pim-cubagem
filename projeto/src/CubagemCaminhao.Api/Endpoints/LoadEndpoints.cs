using CubagemCaminhao.Domain;
using CubagemCaminhao.Domain.Entities;
using CubagemCaminhao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CubagemCaminhao.Api.Endpoints;

// Histórico de cargas: cada saída de caminhão vira um registro com a carga daquele momento.
public static class LoadEndpoints
{
    // Quantos registros a lista devolve no máximo (os mais recentes).
    private const int MaxLoadsPerQuery = 100;

    public static IEndpointRouteBuilder MapLoadEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/loads", ListLoadsAsync).WithTags("Histórico de cargas");
        endpoints.MapPost("/api/trucks/{truckId:int}/loads", DispatchAsync).WithTags("Histórico de cargas");

        return endpoints;
    }

    // GET /api/loads?truckId=1 ou ?driverId=2 (os dois são opcionais).
    private static async Task<IResult> ListLoadsAsync(
        int? truckId,
        int? driverId,
        CubagemDbContext database,
        CancellationToken cancellationToken) =>
        Results.Ok(await GetLoadsAsync(database, truckId, driverId, cancellationToken));

    // Registra a saída: grava a carga atual no histórico e esvazia o caminhão para o próximo carregamento.
    private static async Task<IResult> DispatchAsync(
        int truckId,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        var truck = await database.Trucks
            .Include(item => item.CargoItems)
            .Include(item => item.Driver)
            .FirstOrDefaultAsync(item => item.Id == truckId, cancellationToken);
        if (truck is null)
        {
            return Results.NotFound(new { message = "Caminhão não encontrado." });
        }

        if (truck.CargoItems.Count == 0)
        {
            return Results.BadRequest(new { message = "O caminhão está sem carga. Adicione itens antes de registrar a saída." });
        }

        // Mesmo cálculo que a tela mostra, para o histórico guardar exatamente o que o usuário viu.
        var summary = TruckEndpoints.ToResponse(truck);

        var load = new Load
        {
            DispatchedAt = DateTimeOffset.UtcNow,
            TruckId = truck.Id,
            TruckName = truck.Name,
            TruckPlate = truck.Plate,
            DriverId = truck.DriverId,
            DriverName = truck.Driver?.Name,
            TruckVolumeM3 = summary.TruckVolumeM3,
            MaxLoadKg = truck.MaxLoadKg,
            CargoVolumeM3 = summary.CargoVolumeM3,
            OccupancyPercent = summary.OccupancyPercent,
            TotalWeightKg = summary.TotalWeightKg,
            CubedWeightKg = summary.CubedWeightKg,
            FreightWeightKg = summary.FreightWeightKg,
            Status = summary.Status
        };

        foreach (var cargo in truck.CargoItems.OrderBy(item => item.Name))
        {
            load.Items.Add(new LoadItem
            {
                Name = cargo.Name,
                LengthCm = cargo.LengthCm,
                WidthCm = cargo.WidthCm,
                HeightCm = cargo.HeightCm,
                WeightKg = cargo.WeightKg,
                Quantity = cargo.Quantity
            });
        }

        database.Loads.Add(load);
        database.CargoItems.RemoveRange(truck.CargoItems);

        // Um SaveChanges só: grava o histórico e limpa a carga na mesma transação.
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/loads?truckId={truck.Id}", ToResponse(load));
    }

    internal static async Task<List<LoadResponse>> GetLoadsAsync(
        CubagemDbContext database,
        int? truckId,
        int? driverId,
        CancellationToken cancellationToken)
    {
        var query = database.Loads.AsNoTracking().Include(load => load.Items).AsQueryable();
        if (truckId is not null)
        {
            query = query.Where(load => load.TruckId == truckId);
        }

        if (driverId is not null)
        {
            query = query.Where(load => load.DriverId == driverId);
        }

        var loads = await query
            .OrderByDescending(load => load.DispatchedAt)
            .ThenByDescending(load => load.Id)
            .Take(MaxLoadsPerQuery)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return loads.Select(ToResponse).ToList();
    }

    private static LoadResponse ToResponse(Load load) => new(
        load.Id,
        load.DispatchedAt,
        load.TruckId,
        load.TruckName,
        load.TruckPlate,
        load.DriverId,
        load.DriverName,
        load.TruckVolumeM3,
        load.MaxLoadKg,
        load.CargoVolumeM3,
        load.OccupancyPercent,
        load.TotalWeightKg,
        load.CubedWeightKg,
        load.FreightWeightKg,
        load.Status,
        load.Items
            .OrderBy(item => item.Name)
            .Select(item => new LoadItemResponse(
                item.Name,
                item.LengthCm,
                item.WidthCm,
                item.HeightCm,
                item.WeightKg,
                item.Quantity,
                Math.Round(CubageCalculator.GetVolumeM3(item.LengthCm, item.WidthCm, item.HeightCm) * item.Quantity, 3),
                Math.Round(item.WeightKg * item.Quantity, 2)))
            .ToList());
}

public sealed record LoadResponse(
    int Id,
    DateTimeOffset DispatchedAt,
    int TruckId,
    string TruckName,
    string TruckPlate,
    int? DriverId,
    string? DriverName,
    decimal TruckVolumeM3,
    decimal MaxLoadKg,
    decimal CargoVolumeM3,
    decimal OccupancyPercent,
    decimal TotalWeightKg,
    decimal CubedWeightKg,
    decimal FreightWeightKg,
    string Status,
    IReadOnlyList<LoadItemResponse> Items);

public sealed record LoadItemResponse(
    string Name,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal WeightKg,
    int Quantity,
    decimal TotalVolumeM3,
    decimal TotalWeightKg);

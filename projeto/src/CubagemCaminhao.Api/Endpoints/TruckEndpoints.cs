using CubagemCaminhao.Domain;
using CubagemCaminhao.Domain.Entities;
using CubagemCaminhao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CubagemCaminhao.Api.Endpoints;

public static class TruckEndpoints
{
    public static IEndpointRouteBuilder MapTruckEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/trucks").WithTags("Caminhões");

        group.MapGet("/", GetTrucksAsync);
        group.MapPost("/", CreateTruckAsync);
        group.MapDelete("/{truckId:int}", DeleteTruckAsync);
        group.MapPut("/{truckId:int}/driver", SetDriverAsync);
        group.MapPost("/{truckId:int}/cargo", CreateCargoAsync);
        group.MapDelete("/{truckId:int}/cargo/{cargoId:int}", DeleteCargoAsync);

        return endpoints;
    }

    private static async Task<IResult> GetTrucksAsync(CubagemDbContext database, CancellationToken cancellationToken)
    {
        var trucks = await database.Trucks
            .AsNoTracking()
            .Include(truck => truck.CargoItems)
            .Include(truck => truck.Driver)
            .OrderBy(truck => truck.Name)
            .ToListAsync(cancellationToken);

        return Results.Ok(trucks.Select(ToResponse).ToList());
    }

    private static async Task<IResult> CreateTruckAsync(
        CreateTruckRequest request,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120)
        {
            return Results.BadRequest(new { message = "Informe um nome para o caminhão (até 120 caracteres)." });
        }

        var plate = request.Plate?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(plate) || plate.Length > 10)
        {
            return Results.BadRequest(new { message = "Informe uma placa válida (até 10 caracteres)." });
        }

        if (!HasValidDimensions(request.LengthCm, request.WidthCm, request.HeightCm))
        {
            return Results.BadRequest(new { message = "As dimensões do caminhão devem ser maiores que zero e menores que 100.000 cm." });
        }

        if (request.MaxLoadKg is <= 0 or >= 100_000)
        {
            return Results.BadRequest(new { message = "A capacidade de carga deve ser maior que zero e menor que 100.000 kg." });
        }

        if (await database.Trucks.AnyAsync(truck => truck.Plate == plate, cancellationToken))
        {
            return Results.Conflict(new { message = "Já existe um caminhão cadastrado com essa placa." });
        }

        var driverError = await ValidateDriverAsync(request.DriverId, null, database, cancellationToken);
        if (driverError is not null)
        {
            return driverError;
        }

        var truck = new Truck
        {
            Name = request.Name.Trim(),
            Plate = plate,
            LengthCm = request.LengthCm,
            WidthCm = request.WidthCm,
            HeightCm = request.HeightCm,
            MaxLoadKg = request.MaxLoadKg,
            DriverId = request.DriverId
        };

        database.Trucks.Add(truck);
        await database.SaveChangesAsync(cancellationToken);
        await database.Entry(truck).Reference(item => item.Driver).LoadAsync(cancellationToken);

        return Results.Created($"/api/trucks/{truck.Id}", ToResponse(truck));
    }

    private static async Task<IResult> DeleteTruckAsync(
        int truckId,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        var truck = await database.Trucks.FindAsync([truckId], cancellationToken);
        if (truck is null)
        {
            return Results.NotFound(new { message = "Caminhão não encontrado." });
        }

        database.Trucks.Remove(truck);
        await database.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    // Troca o motorista do caminhão. DriverId null deixa o caminhão sem motorista.
    private static async Task<IResult> SetDriverAsync(
        int truckId,
        SetDriverRequest request,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        var truck = await database.Trucks
            .Include(item => item.CargoItems)
            .FirstOrDefaultAsync(item => item.Id == truckId, cancellationToken);
        if (truck is null)
        {
            return Results.NotFound(new { message = "Caminhão não encontrado." });
        }

        var driverError = await ValidateDriverAsync(request.DriverId, truckId, database, cancellationToken);
        if (driverError is not null)
        {
            return driverError;
        }

        truck.DriverId = request.DriverId;
        await database.SaveChangesAsync(cancellationToken);
        await database.Entry(truck).Reference(item => item.Driver).LoadAsync(cancellationToken);

        return Results.Ok(ToResponse(truck));
    }

    // Confere se o motorista existe e se já não está em outro caminhão.
    private static async Task<IResult?> ValidateDriverAsync(
        int? driverId,
        int? truckId,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        if (driverId is null)
        {
            return null;
        }

        if (!await database.Drivers.AnyAsync(driver => driver.Id == driverId, cancellationToken))
        {
            return Results.BadRequest(new { message = "Motorista não encontrado." });
        }

        var otherTruckPlate = await database.Trucks
            .Where(truck => truck.DriverId == driverId && truck.Id != truckId)
            .Select(truck => truck.Plate)
            .FirstOrDefaultAsync(cancellationToken);
        if (otherTruckPlate is not null)
        {
            return Results.Conflict(new { message = $"Esse motorista já está no caminhão {otherTruckPlate}." });
        }

        return null;
    }

    private static async Task<IResult> CreateCargoAsync(
        int truckId,
        CreateCargoRequest request,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        var truck = await database.Trucks.FindAsync([truckId], cancellationToken);
        if (truck is null)
        {
            return Results.NotFound(new { message = "Caminhão não encontrado." });
        }

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120)
        {
            return Results.BadRequest(new { message = "Informe um nome para a carga (até 120 caracteres)." });
        }

        if (!HasValidDimensions(request.LengthCm, request.WidthCm, request.HeightCm))
        {
            return Results.BadRequest(new { message = "As dimensões da carga devem ser maiores que zero e menores que 100.000 cm." });
        }

        if (request.WeightKg is <= 0 or >= 100_000)
        {
            return Results.BadRequest(new { message = "O peso por unidade deve ser maior que zero e menor que 100.000 kg." });
        }

        if (request.Quantity is < 1 or > 100_000)
        {
            return Results.BadRequest(new { message = "A quantidade deve estar entre 1 e 100.000 unidades." });
        }

        // Cada item precisa caber sozinho no baú. Volume e peso acima do limite são aceitos, com alerta.
        var fitError = CubageCalculator.GetFitError(
            request.LengthCm, request.WidthCm, request.HeightCm,
            truck.LengthCm, truck.WidthCm, truck.HeightCm);
        if (fitError is not null)
        {
            return Results.BadRequest(new { message = fitError });
        }

        var cargo = new CargoItem
        {
            Name = request.Name.Trim(),
            LengthCm = request.LengthCm,
            WidthCm = request.WidthCm,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            Quantity = request.Quantity,
            TruckId = truckId
        };

        database.CargoItems.Add(cargo);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/trucks/{truckId}/cargo/{cargo.Id}", ToCargoResponse(cargo));
    }

    private static async Task<IResult> DeleteCargoAsync(
        int truckId,
        int cargoId,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        var cargo = await database.CargoItems
            .FirstOrDefaultAsync(item => item.Id == cargoId && item.TruckId == truckId, cancellationToken);
        if (cargo is null)
        {
            return Results.NotFound(new { message = "Carga não encontrada neste caminhão." });
        }

        database.CargoItems.Remove(cargo);
        await database.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static bool HasValidDimensions(decimal lengthCm, decimal widthCm, decimal heightCm) =>
        lengthCm is > 0 and < 100_000
        && widthCm is > 0 and < 100_000
        && heightCm is > 0 and < 100_000;

    private static CargoResponse ToCargoResponse(CargoItem item) => new(
        item.Id,
        item.Name,
        item.LengthCm,
        item.WidthCm,
        item.HeightCm,
        item.WeightKg,
        item.Quantity,
        Math.Round(CubageCalculator.GetVolumeM3(item.LengthCm, item.WidthCm, item.HeightCm) * item.Quantity, 3),
        Math.Round(item.WeightKg * item.Quantity, 2));

    // Os resultados não são gravados no banco: são calculados aqui a cada consulta.
    internal static TruckResponse ToResponse(Truck truck)
    {
        var truckVolumeM3 = CubageCalculator.GetVolumeM3(truck.LengthCm, truck.WidthCm, truck.HeightCm);
        var cargo = truck.CargoItems
            .OrderBy(item => item.Name)
            .Select(ToCargoResponse)
            .ToList();
        var cargoVolumeM3 = cargo.Sum(item => item.TotalVolumeM3);
        var totalWeightKg = cargo.Sum(item => item.TotalWeightKg);
        var cubedWeightKg = CubageCalculator.GetCubedWeightKg(cargoVolumeM3);

        return new TruckResponse(
            truck.Id,
            truck.Name,
            truck.Plate,
            truck.LengthCm,
            truck.WidthCm,
            truck.HeightCm,
            truck.MaxLoadKg,
            truck.DriverId,
            truck.Driver?.Name,
            Math.Round(truckVolumeM3, 3),
            Math.Round(CubageCalculator.GetUsableVolumeM3(truckVolumeM3), 3),
            Math.Round(cargoVolumeM3, 3),
            Math.Round(CubageCalculator.GetOccupancyPercent(cargoVolumeM3, truckVolumeM3), 1),
            totalWeightKg,
            Math.Round(cubedWeightKg, 2),
            Math.Round(CubageCalculator.GetFreightWeightKg(totalWeightKg, cubedWeightKg), 2),
            CubageCalculator.GetStatus(cargoVolumeM3, truckVolumeM3, totalWeightKg, truck.MaxLoadKg),
            cargo);
    }
}

public sealed record CreateTruckRequest(
    string? Name,
    string? Plate,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal MaxLoadKg,
    int? DriverId);

public sealed record SetDriverRequest(int? DriverId);

public sealed record CreateCargoRequest(
    string? Name,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal WeightKg,
    int Quantity);

public sealed record TruckResponse(
    int Id,
    string Name,
    string Plate,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal MaxLoadKg,
    int? DriverId,
    string? DriverName,
    decimal TruckVolumeM3,
    decimal UsableVolumeM3,
    decimal CargoVolumeM3,
    decimal OccupancyPercent,
    decimal TotalWeightKg,
    decimal CubedWeightKg,
    decimal FreightWeightKg,
    string Status,
    IReadOnlyList<CargoResponse> CargoItems);

public sealed record CargoResponse(
    int Id,
    string Name,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal WeightKg,
    int Quantity,
    decimal TotalVolumeM3,
    decimal TotalWeightKg);

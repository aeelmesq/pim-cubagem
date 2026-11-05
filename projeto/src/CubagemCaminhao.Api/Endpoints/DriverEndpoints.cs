using CubagemCaminhao.Domain.Entities;
using CubagemCaminhao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CubagemCaminhao.Api.Endpoints;

public static class DriverEndpoints
{
    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/drivers").WithTags("Motoristas");

        group.MapGet("/", GetDriversAsync);
        group.MapPost("/", CreateDriverAsync);
        group.MapDelete("/{driverId:int}", DeleteDriverAsync);

        return endpoints;
    }

    private static async Task<IResult> GetDriversAsync(CubagemDbContext database, CancellationToken cancellationToken)
    {
        var drivers = await database.Drivers
            .AsNoTracking()
            .OrderBy(driver => driver.Name)
            .Select(driver => new DriverResponse(
                driver.Id,
                driver.Name,
                database.Trucks.Where(truck => truck.DriverId == driver.Id).Select(truck => truck.Plate).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Results.Ok(drivers);
    }

    private static async Task<IResult> CreateDriverAsync(
        CreateDriverRequest request,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120)
        {
            return Results.BadRequest(new { message = "Informe o nome do motorista (até 120 caracteres)." });
        }

        var driver = new Driver { Name = request.Name.Trim() };

        database.Drivers.Add(driver);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/drivers/{driver.Id}", new DriverResponse(driver.Id, driver.Name, null));
    }

    // O caminhão do motorista fica sem motorista (DeleteBehavior.SetNull).
    private static async Task<IResult> DeleteDriverAsync(
        int driverId,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        var driver = await database.Drivers.FindAsync([driverId], cancellationToken);
        if (driver is null)
        {
            return Results.NotFound(new { message = "Motorista não encontrado." });
        }

        database.Drivers.Remove(driver);
        await database.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}

public sealed record CreateDriverRequest(string? Name);

// TruckPlate: placa do caminhão do motorista, ou null se ele estiver sem caminhão.
public sealed record DriverResponse(int Id, string Name, string? TruckPlate);

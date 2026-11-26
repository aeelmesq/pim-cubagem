using CubagemCaminhao.Api.Auth;
using CubagemCaminhao.Domain.Entities;
using CubagemCaminhao.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CubagemCaminhao.Api.Endpoints;

public static class DriverEndpoints
{
    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/drivers").WithTags("Motoristas");

        group.MapGet("/", GetDriversAsync);
        group.MapPost("/", CreateDriverAsync);
        group.MapPut("/{driverId:int}/password", SetPasswordAsync);
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
                driver.Username,
                database.Trucks.Where(truck => truck.DriverId == driver.Id).Select(truck => truck.Plate).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Results.Ok(drivers);
    }

    private static async Task<IResult> CreateDriverAsync(
        CreateDriverRequest request,
        CubagemDbContext database,
        IPasswordHasher<Driver> passwordHasher,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120)
        {
            return Results.BadRequest(new { message = "Informe o nome do motorista (até 120 caracteres)." });
        }

        var username = DriverCredentials.NormalizeUsername(request.Username);
        var credentialsError = DriverCredentials.GetUsernameError(username) ?? DriverCredentials.GetPasswordError(request.Password);
        if (credentialsError is not null)
        {
            return Results.BadRequest(new { message = credentialsError });
        }

        if (await database.Drivers.AnyAsync(driver => driver.Username == username, cancellationToken))
        {
            return Results.Conflict(new { message = "Já existe um motorista com esse usuário." });
        }

        var driver = new Driver { Name = request.Name.Trim(), Username = username, PasswordHash = string.Empty };
        driver.PasswordHash = passwordHasher.HashPassword(driver, request.Password!);

        database.Drivers.Add(driver);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/drivers/{driver.Id}", new DriverResponse(driver.Id, driver.Name, driver.Username, null));
    }

    // Troca a senha do motorista (por exemplo, quando ele esquece).
    private static async Task<IResult> SetPasswordAsync(
        int driverId,
        SetPasswordRequest request,
        CubagemDbContext database,
        IPasswordHasher<Driver> passwordHasher,
        CancellationToken cancellationToken)
    {
        var driver = await database.Drivers.FindAsync([driverId], cancellationToken);
        if (driver is null)
        {
            return Results.NotFound(new { message = "Motorista não encontrado." });
        }

        var passwordError = DriverCredentials.GetPasswordError(request.Password);
        if (passwordError is not null)
        {
            return Results.BadRequest(new { message = passwordError });
        }

        driver.PasswordHash = passwordHasher.HashPassword(driver, request.Password!);
        await database.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    // O caminhão do motorista fica sem motorista (DeleteBehavior.SetNull). O histórico continua com o nome dele.
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

public sealed record CreateDriverRequest(string? Name, string? Username, string? Password);

public sealed record SetPasswordRequest(string? Password);

// TruckPlate: placa do caminhão do motorista, ou null se ele estiver sem caminhão.
public sealed record DriverResponse(int Id, string Name, string Username, string? TruckPlate);

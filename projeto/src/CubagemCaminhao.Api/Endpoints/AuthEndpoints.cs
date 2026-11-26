using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CubagemCaminhao.Api.Auth;
using CubagemCaminhao.Domain.Entities;
using CubagemCaminhao.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CubagemCaminhao.Api.Endpoints;

// Login do motorista, área "minha carga" e login do administrador.
public static class AuthEndpoints
{
    private const string InvalidCredentialsMessage = "Usuário ou senha inválidos.";

    // Hash de uma senha qualquer, usado quando o usuário não existe: assim a resposta demora o mesmo
    // tempo nos dois casos e não dá para descobrir quais usuários existem medindo o tempo.
    private static readonly Driver DummyDriver = new() { Name = "-", Username = "-", PasswordHash = "-" };
    private static readonly string DummyHash = new PasswordHasher<Driver>().HashPassword(DummyDriver, "senha-que-nao-existe");

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth").WithTags("Login");
        auth.MapPost("/login", LoginAsync).RequireRateLimiting(AuthSetup.LoginRateLimitPolicy);
        auth.MapPost("/logout", Logout);
        auth.MapPost("/admin/login", AdminLogin).RequireRateLimiting(AuthSetup.LoginRateLimitPolicy);

        endpoints.MapGet("/api/me", GetMeAsync)
            .WithTags("Login")
            .RequireAuthorization(policy => policy.RequireRole(AuthSetup.DriverRole));

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext http,
        CubagemDbContext database,
        IPasswordHasher<Driver> passwordHasher,
        TokenService tokenService,
        CancellationToken cancellationToken)
    {
        var username = DriverCredentials.NormalizeUsername(request.Username);
        var password = request.Password ?? string.Empty;

        var driver = username.Length == 0
            ? null
            : await database.Drivers.FirstOrDefaultAsync(item => item.Username == username, cancellationToken);
        if (driver is null)
        {
            passwordHasher.VerifyHashedPassword(DummyDriver, DummyHash, password);
            return InvalidCredentials();
        }

        var result = passwordHasher.VerifyHashedPassword(driver, driver.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return InvalidCredentials();
        }

        // Hash feito com parâmetros antigos (menos iterações): grava de novo com os atuais.
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            driver.PasswordHash = passwordHasher.HashPassword(driver, password);
            await database.SaveChangesAsync(cancellationToken);
        }

        var (token, expiresAt) = tokenService.CreateToken(driver);
        http.Response.Cookies.Append(AuthSetup.CookieName, token, AuthSetup.CreateCookieOptions(http, expiresAt));

        // O token também volta no corpo para o Blazor, que chama a API pelo servidor e guarda
        // o token no cookie HttpOnly do próprio site. No navegador, basta o cookie.
        return Results.Ok(new LoginResponse(token, expiresAt, driver.Id, driver.Name));
    }

    private static IResult Logout(HttpContext http)
    {
        http.Response.Cookies.Delete(AuthSetup.CookieName, AuthSetup.CreateCookieOptions(http));
        return Results.NoContent();
    }

    // Dados do motorista logado: o caminhão dele (com a carga atual) e o histórico das cargas que ele levou.
    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal user,
        CubagemDbContext database,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var driverId))
        {
            return Results.Unauthorized();
        }

        var driver = await database.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == driverId, cancellationToken);
        if (driver is null)
        {
            // Token válido, mas o motorista foi excluído depois do login.
            return Results.Unauthorized();
        }

        var truck = await database.Trucks
            .AsNoTracking()
            .Include(item => item.CargoItems)
            .Include(item => item.Driver)
            .FirstOrDefaultAsync(item => item.DriverId == driverId, cancellationToken);

        var loads = await LoadEndpoints.GetLoadsAsync(database, truckId: null, driverId, cancellationToken);

        return Results.Ok(new MeResponse(
            driver.Id,
            driver.Name,
            driver.Username,
            truck is null ? null : TruckEndpoints.ToResponse(truck),
            loads));
    }

    // Confere usuário e senha do administrador (configurados na API, fora do código em produção).
    // A sessão do administrador fica no cookie do Blazor; a API só confere as credenciais.
    private static IResult AdminLogin(LoginRequest request, AdminOptions admin)
    {
        if (string.IsNullOrEmpty(admin.Password))
        {
            return Results.Json(
                new { message = "O login de administrador não está configurado na API (Admin:Password)." },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var expectedUsername = DriverCredentials.NormalizeUsername(admin.Username);
        var usernameOk = FixedTimeEquals(DriverCredentials.NormalizeUsername(request.Username), expectedUsername);
        var passwordOk = FixedTimeEquals(request.Password ?? string.Empty, admin.Password);

        // "&" e não "&&": confere as duas coisas sempre, para a resposta demorar o mesmo tempo.
        if (!(usernameOk & passwordOk))
        {
            return InvalidCredentials();
        }

        return Results.Ok(new AdminLoginResponse(expectedUsername));
    }

    // Compara em tempo constante (o resumo SHA-256 deixa os dois lados do mesmo tamanho).
    private static bool FixedTimeEquals(string value, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));

    private static IResult InvalidCredentials() =>
        Results.Json(new { message = InvalidCredentialsMessage }, statusCode: StatusCodes.Status401Unauthorized);
}

public sealed record LoginRequest(string? Username, string? Password);

public sealed record AdminLoginResponse(string Username);

public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt, int DriverId, string DriverName);

public sealed record MeResponse(
    int Id,
    string Name,
    string Username,
    TruckResponse? Truck,
    IReadOnlyList<LoadResponse> Loads);

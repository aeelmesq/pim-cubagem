using System.Globalization;

using Microsoft.EntityFrameworkCore;
using CubagemCaminhao.Infrastructure.Data;
using CubagemCaminhao.Api.Auth;
using CubagemCaminhao.Api.Endpoints;

namespace CubagemCaminhao.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Mensagens com número no formato brasileiro (12,5 cm), mesmo em servidor sem idioma configurado.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];
        builder.Services.AddCors(options =>
            options.AddPolicy("ClientApplications", policy =>
                policy.SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    {
                        return false;
                    }

                    if (builder.Environment.IsDevelopment())
                    {
                        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);
                    }

                    return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                // Deixa o navegador mandar o cookie do login (só para as origens permitidas acima).
                .AllowCredentials()));
        builder.AddDriverAuthentication();
        builder.Services.AddDbContext<CubagemDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Configure a connection string ConnectionStrings:DefaultConnection (veja o projeto/README.md).")));

        var app = builder.Build();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<CubagemDbContext>();
            await database.Database.EnsureCreatedAsync();
        }

        app.UseCors("ClientApplications");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapHealthChecks("/health");
        app.MapTruckEndpoints();
        app.MapDriverEndpoints();
        app.MapLoadEndpoints();
        app.MapAuthEndpoints();
        app.MapGet("/api/status", () => new
        {
            application = "CubagemCaminhao.Api",
            status = "ready",
            architecture = "Blazor + Flutter -> ASP.NET Core API -> SQL Server"
        });

        app.Run();
    }
}

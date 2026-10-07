
using Microsoft.EntityFrameworkCore;
using CubagemCaminhao.Infrastructure.Data;
using CubagemCaminhao.Api.Endpoints;

namespace CubagemCaminhao.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
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
                .AllowAnyMethod()));
        builder.Services.AddDbContext<CubagemDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("A connection string named 'DefaultConnection' is required.")));

        var app = builder.Build();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<CubagemDbContext>();
            await database.Database.EnsureCreatedAsync();
        }

        app.UseCors("ClientApplications");

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapHealthChecks("/health");
        app.MapTruckEndpoints();
        app.MapGet("/api/status", () => new
        {
            application = "CubagemCaminhao.Api",
            status = "ready",
            architecture = "Blazor + Flutter -> ASP.NET Core API -> SQL Server"
        });

        app.Run();
    }
}

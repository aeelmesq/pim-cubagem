using System.Text;
using System.Threading.RateLimiting;
using CubagemCaminhao.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CubagemCaminhao.Api.Auth;

// Configuração do JWT (seção "Jwt" do appsettings ou variáveis Jwt__Key etc.).
public sealed class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CubagemCaminhao.Api";
    public string Audience { get; set; } = "CubagemCaminhao";
    public int ExpirationHours { get; set; } = 8;

    public SymmetricSecurityKey GetSigningKey() => new(Encoding.UTF8.GetBytes(Key));
}

public static class AuthSetup
{
    // Nome do cookie HttpOnly que guarda o JWT no navegador.
    public const string CookieName = "cubagem_token";

    public const string DriverRole = "Motorista";
    public const string LoginRateLimitPolicy = "login";

    public static void AddDriverAuthentication(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

        // HMAC-SHA256 precisa de uma chave de pelo menos 256 bits.
        if (Encoding.UTF8.GetByteCount(options.Key) < 32)
        {
            throw new InvalidOperationException(
                "Configure a chave do JWT em Jwt:Key com pelo menos 32 caracteres (veja o projeto/README.md).");
        }

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<TokenService>();
        builder.Services.AddSingleton<IPasswordHasher<Driver>, PasswordHasher<Driver>>();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                // Mantém os nomes curtos do JWT ("sub", "name", "role").
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = options.GetSigningKey(),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = JwtRegisteredClaimNames.Name,
                    RoleClaimType = "role"
                };
                jwt.Events = new JwtBearerEvents
                {
                    // O token pode vir no cabeçalho "Authorization: Bearer ..." (Blazor, que chama a API pelo servidor)
                    // ou no cookie HttpOnly (navegador chamando a API direto).
                    OnMessageReceived = context =>
                    {
                        if (!context.Request.Headers.ContainsKey("Authorization")
                            && context.Request.Cookies.TryGetValue(CookieName, out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // Contra tentativa de adivinhar senha: 10 tentativas de login por minuto por IP.
        builder.Services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy(LoginRateLimitPolicy, http => RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
        });
    }

    public static CookieOptions CreateCookieOptions(HttpContext http, DateTimeOffset? expiresAt = null) => new()
    {
        // HttpOnly: o JavaScript da página não consegue ler o token (protege contra XSS).
        HttpOnly = true,
        // Secure: só vai por HTTPS. Atrás do nginx, IsHttps vem do X-Forwarded-Proto.
        Secure = http.Request.IsHttps,
        // Strict: o navegador não manda o cookie em requisições vindas de outros sites (protege contra CSRF).
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = expiresAt,
        IsEssential = true
    };
}

public sealed class TokenService(JwtOptions options)
{
    public (string Token, DateTimeOffset ExpiresAt) CreateToken(Driver driver)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddHours(options.ExpirationHours);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = driver.Id.ToString(),
                [JwtRegisteredClaimNames.Name] = driver.Name,
                ["role"] = AuthSetup.DriverRole
            },
            SigningCredentials = new SigningCredentials(options.GetSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}

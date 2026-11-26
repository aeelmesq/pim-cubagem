namespace CubagemCaminhao.Blazor;

// Cookie do login do motorista. Guarda o JWT que a API devolveu.
// Mesmo nome que a API usa, assim o navegador também consegue chamar /api direto (app web).
public static class DriverSession
{
    public const string CookieName = "cubagem_token";

    public static CookieOptions CreateCookieOptions(HttpContext http, DateTimeOffset? expiresAt = null) => new()
    {
        // O JavaScript da página não lê o cookie (protege contra XSS).
        HttpOnly = true,
        // Só por HTTPS quando o site está em HTTPS (atrás do nginx, vem do X-Forwarded-Proto).
        Secure = http.Request.IsHttps,
        // Não é enviado em requisições vindas de outros sites (protege contra CSRF).
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = expiresAt,
        IsEssential = true
    };
}

using System.Globalization;
using CubagemCaminhao.Blazor;
using CubagemCaminhao.Blazor.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

// Números e datas no formato brasileiro (52,7%, 3.000 kg), mesmo em servidor sem idioma configurado.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("CubagemApi", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:8080";
    client.BaseAddress = new Uri(baseUrl);
})
// Sem isto o HttpClient guardaria o cookie do login que a API devolve e o mandaria nas chamadas
// de todos os usuários do site. O token de cada motorista vai só no cabeçalho Authorization.
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false });

// Login do administrador: as telas de cadastro (Início, Caminhões, Motoristas, Histórico) exigem este cookie.
// A área do motorista continua com o login próprio (cookie cubagem_token).
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = AdminSession.CookieName;
        // O JavaScript da página não lê o cookie (XSS) e outros sites não o enviam (CSRF).
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = AdminSession.LoginPath;
        options.AccessDeniedPath = AdminSession.LoginPath;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

// Sair do login de administrador.
app.MapPost("/admin/sair", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect(AdminSession.LoginPath);
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

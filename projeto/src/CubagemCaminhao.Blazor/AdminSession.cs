namespace CubagemCaminhao.Blazor;

// Login do administrador, que dá acesso às telas de cadastro.
public static class AdminSession
{
    public const string Role = "Administrador";
    public const string CookieName = "cubagem_admin";
    public const string LoginPath = "/admin/entrar";
}

namespace CubagemCaminhao.Domain.Entities;

// Só nome e login: o sistema não precisa de CPF, CNH ou telefone (LGPD, minimização).
public sealed class Driver
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Usuário para entrar no sistema (único, em minúsculas).
    public required string Username { get; set; }

    // Hash da senha (PBKDF2 com salt, do PasswordHasher do ASP.NET Core). A senha em si nunca é gravada.
    public required string PasswordHash { get; set; }
}

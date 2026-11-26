using System.Text.RegularExpressions;

namespace CubagemCaminhao.Api.Auth;

// Regras de usuário e senha do motorista.
public static partial class DriverCredentials
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 100;

    public static string NormalizeUsername(string? username) => username?.Trim().ToLowerInvariant() ?? string.Empty;

    public static string? GetUsernameError(string username) =>
        UsernamePattern().IsMatch(username)
            ? null
            : "O usuário deve ter de 3 a 40 caracteres: letras sem acento, números, ponto, hífen ou sublinhado.";

    public static string? GetPasswordError(string? password) =>
        password is { Length: >= PasswordMinLength and <= PasswordMaxLength }
            ? null
            : $"A senha deve ter de {PasswordMinLength} a {PasswordMaxLength} caracteres.";

    [GeneratedRegex("^[a-z0-9._-]{3,40}$")]
    private static partial Regex UsernamePattern();
}

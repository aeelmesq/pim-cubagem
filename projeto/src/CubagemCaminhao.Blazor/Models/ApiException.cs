using System.Net.Http.Json;

namespace CubagemCaminhao.Blazor.Models;

// Erro devolvido pela API, com a mensagem pronta para mostrar ao usuário.
public sealed class ApiException(string message) : Exception(message)
{
    public const string ConnectionError = "Não foi possível falar com a API. Confira se ela está rodando.";

    public static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = "A API recusou a operação.";
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiErrorBody>();
            if (!string.IsNullOrWhiteSpace(body?.Message))
            {
                message = body.Message;
            }
        }
        catch
        {
            // Resposta sem JSON: fica a mensagem padrão.
        }

        throw new ApiException(message);
    }

    private sealed record ApiErrorBody(string? Message);
}

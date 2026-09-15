using System.Net;
using System.Text.Json;

namespace CarrinhoCompras.Api.IntegrationTests.Suporte;

internal static class ProblemDetailsAssertions
{
    /// <summary>
    /// Verifica o contrato único de erro da API: status HTTP, <c>application/problem+json</c> e os campos
    /// <c>type</c>, <c>title</c>, <c>status</c>, <c>detail</c>, <c>instance</c>, <c>traceId</c> e <c>code</c>.
    /// </summary>
    public static async Task<JsonElement> DeveSerProblemaAsync(this HttpResponseMessage resposta, HttpStatusCode status, string code)
    {
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        resposta.StatusCode.ShouldBe(status, $"Corpo recebido: {corpo}");
        resposta.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problema = JsonDocument.Parse(corpo).RootElement.Clone();
        problema.GetProperty("status").GetInt32().ShouldBe((int)status);
        problema.GetProperty("code").GetString().ShouldBe(code);
        problema.GetProperty("type").GetString().ShouldNotBeNullOrWhiteSpace();
        problema.GetProperty("title").GetString().ShouldNotBeNullOrWhiteSpace();
        problema.GetProperty("detail").GetString().ShouldNotBeNullOrWhiteSpace();
        problema.GetProperty("instance").GetString().ShouldNotBeNullOrWhiteSpace();
        problema.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        return problema;
    }

    public static string Detalhe(this JsonElement problema) => problema.GetProperty("detail").GetString()!;

    /// <summary>Mensagens de validação de um campo (formato <c>errors: { campo: [mensagens] }</c>).</summary>
    public static IReadOnlyList<string> ErrosDoCampo(this JsonElement problema, string campo) =>
        [.. problema.GetProperty("errors").GetProperty(campo).EnumerateArray().Select(mensagem => mensagem.GetString()!)];
}

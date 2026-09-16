using System.Net;
using System.Text.Json;
using CarrinhoCompras.Api.IntegrationTests.Suporte;

namespace CarrinhoCompras.Api.IntegrationTests.Plataforma;

public sealed class SaudeEDocumentacaoTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Health_check_responde_saudavel_com_banco_acessivel()
    {
        var resposta = await _cliente.GetAsync("/health", TestContext.Current.CancellationToken);

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBe("Healthy");
    }

    [Fact]
    public async Task Documento_openapi_descreve_os_endpoints_com_status_como_texto_e_dinheiro_como_decimal()
    {
        var resposta = await _cliente.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var raiz = documento.RootElement;
        var caminhos = raiz.GetProperty("paths").EnumerateObject().Select(caminho => caminho.Name).ToList();
        caminhos.ShouldBe(
            new[]
            {
                "/api/carrinhos",
                "/api/carrinhos/{carrinhoId}",
                "/api/carrinhos/{carrinhoId}/finalizar",
                "/api/carrinhos/{carrinhoId}/itens",
                "/api/carrinhos/{carrinhoId}/itens/{produtoId}",
                "/api/carrinhos/{carrinhoId}/cupom",
                "/api/produtos",
                "/api/produtos/{produtoId}",
            },
            ignoreOrder: true);

        var esquemas = raiz.GetProperty("components").GetProperty("schemas");
        esquemas.GetProperty("StatusCarrinho").GetProperty("enum").EnumerateArray().Select(valor => valor.GetString())
            .ShouldBe(new[] { "Aberto", "Finalizado", "Expirado" });
        esquemas.GetProperty("CarrinhoResponse").GetProperty("properties").GetProperty("total").GetProperty("format").GetString()
            .ShouldBe("decimal");
    }
}

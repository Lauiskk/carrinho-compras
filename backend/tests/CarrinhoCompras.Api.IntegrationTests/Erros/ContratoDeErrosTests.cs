using System.Net;
using CarrinhoCompras.Api.IntegrationTests.Suporte;

namespace CarrinhoCompras.Api.IntegrationTests.Erros;

/// <summary>Até os erros gerados pelo próprio framework seguem o formato padronizado da API.</summary>
public sealed class ContratoDeErrosTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Rota_inexistente_retorna_404_no_formato_padrao()
    {
        var resposta = await _cliente.GetAsync("/api/nao-existe", TestContext.Current.CancellationToken);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.NotFound, "recurso.nao_encontrado");
        problema.GetProperty("title").GetString().ShouldBe("Recurso não encontrado");
    }

    [Fact]
    public async Task Metodo_nao_suportado_retorna_405_no_formato_padrao()
    {
        var resposta = await _cliente.DeleteAsync("/api/produtos", TestContext.Current.CancellationToken);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.MethodNotAllowed, "metodo.nao_permitido");
    }
}

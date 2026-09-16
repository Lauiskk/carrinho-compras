using System.Net;
using System.Text.Json;
using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Carrinhos;
using CarrinhoCompras.Domain.Carrinhos;

namespace CarrinhoCompras.Api.IntegrationTests.Carrinhos;

public sealed class CarrinhoTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Criar_carrinho_retorna_201_com_location_e_carrinho_aberto_zerado()
    {
        var resposta = await _cliente.PostAsync("/api/carrinhos", content: null, TestContext.Current.CancellationToken);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Created);
        var carrinho = await resposta.LerAsync<CarrinhoResponse>();
        resposta.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldBe($"/api/carrinhos/{carrinho.Id}");
        carrinho.Status.ShouldBe(StatusCarrinho.Aberto);
        carrinho.Itens.ShouldBeEmpty();
        carrinho.Cupom.ShouldBeNull();
        carrinho.Subtotal.ShouldBe(0m);
        carrinho.Desconto.ShouldBe(0m);
        carrinho.Total.ShouldBe(0m);
        carrinho.FinalizadoEm.ShouldBeNull();
    }

    [Fact]
    public async Task Carrinho_criado_pode_ser_consultado_e_persiste_entre_requisicoes()
    {
        var criado = await _cliente.CriarCarrinhoAsync();

        var consultado = await _cliente.ObterCarrinhoComSucessoAsync(criado.Id);

        // Inclui CriadoEm: a data devolvida na criação é exatamente a que foi persistida.
        consultado.ShouldBeEquivalentTo(criado);
    }

    [Fact]
    public async Task Carrinho_inexistente_retorna_404()
    {
        var resposta = await _cliente.ObterCarrinhoAsync(Guid.CreateVersion7());

        await resposta.DeveSerProblemaAsync(HttpStatusCode.NotFound, "carrinho.nao_encontrado");
    }

    [Fact]
    public async Task Id_de_carrinho_que_nao_e_guid_retorna_400_indicando_o_campo()
    {
        var resposta = await _cliente.GetAsync("/api/carrinhos/nao-e-um-guid", TestContext.Current.CancellationToken);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.BadRequest, "requisicao.invalida");
        problema.ErrosDoCampo("carrinhoId").ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Contrato_json_do_carrinho_usa_os_nomes_esperados_e_status_como_texto()
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();
        var produto = await _cliente.ProdutoComDisponivelAsync(1);
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id);

        var resposta = await _cliente.ObterCarrinhoAsync(carrinho.Id);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var raiz = json.RootElement;

        NomesDasPropriedades(raiz).ShouldBe(
            new[] { "id", "status", "itens", "cupom", "subtotal", "desconto", "total", "criadoEm", "finalizadoEm", "expiraEm" },
            ignoreOrder: true);
        raiz.GetProperty("status").GetString().ShouldBe("Aberto");
        NomesDasPropriedades(raiz.GetProperty("itens")[0]).ShouldBe(
            new[] { "produtoId", "descricaoProduto", "precoLiquidoUnitario", "quantidadeEstoque", "quantidadeDisponivel", "quantidade", "precoItem" },
            ignoreOrder: true);
    }

    private static string[] NomesDasPropriedades(JsonElement objeto) => [.. objeto.EnumerateObject().Select(p => p.Name)];
}

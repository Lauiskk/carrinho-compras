using System.Net;
using System.Net.Http.Json;
using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Produtos;
using CatalogoDeReferencia = CarrinhoCompras.Api.IntegrationTests.Suporte.Catalogo;

namespace CarrinhoCompras.Api.IntegrationTests.Catalogo;

public sealed class ProdutosTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Lista_o_catalogo_com_os_mesmos_valores_do_produtos_json()
    {
        var produtos = await _cliente.ListarProdutosAsync();

        var esperados = CatalogoDeReferencia.Produtos
            .OrderBy(produto => produto.Id)
            .Select(produto => new ProdutoResponse(produto.Id, produto.DescricaoProduto, produto.PrecoLiquido, produto.QuantidadeEstoque));
        produtos.ShouldBe(esperados);
    }

    [Fact]
    public async Task Obtem_um_produto_expondo_preco_liquido_e_estoque()
    {
        var referencia = CatalogoDeReferencia.Produtos[0];

        var produto = await _cliente.GetFromJsonAsync<ProdutoResponse>(
            $"/api/produtos/{referencia.Id}", ClienteApi.Json, TestContext.Current.CancellationToken);

        produto.ShouldNotBeNull();
        produto.PrecoLiquido.ShouldBe(referencia.PrecoLiquido);
        produto.QuantidadeEstoque.ShouldBe(referencia.QuantidadeEstoque);
    }

    [Fact]
    public async Task Produto_inexistente_retorna_404()
    {
        var resposta = await _cliente.GetAsync($"/api/produtos/{CatalogoDeReferencia.IdInexistente}", TestContext.Current.CancellationToken);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.NotFound, "produto.nao_encontrado");
    }

    [Fact]
    public async Task Id_de_produto_invalido_retorna_400_indicando_o_campo()
    {
        var resposta = await _cliente.GetAsync("/api/produtos/abc", TestContext.Current.CancellationToken);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.BadRequest, "requisicao.invalida");
        problema.ErrosDoCampo("produtoId").ShouldNotBeEmpty();
    }
}

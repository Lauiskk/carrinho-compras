using System.Net;
using System.Net.Http.Json;
using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Produtos;
using CatalogoDeReferencia = CarrinhoCompras.Api.IntegrationTests.Suporte.Catalogo;

namespace CarrinhoCompras.Api.IntegrationTests.Catalogo;

public sealed class ProdutosTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    /// <summary>
    /// Identidade do catálogo (id, descrição e preço) idêntica ao <c>produtos.json</c>. As quantidades não
    /// entram na comparação de propósito: com reserva de estoque elas são dado vivo — sobem e descem
    /// enquanto outras sacolas existem e outras compras são finalizadas.
    /// </summary>
    [Fact]
    public async Task Lista_o_catalogo_com_os_mesmos_produtos_do_produtos_json()
    {
        var produtos = await _cliente.ListarProdutosAsync();

        produtos.Select(produto => produto.Identidade())
            .ShouldBe(CatalogoDeReferencia.Produtos.OrderBy(produto => produto.Id).Select(produto => produto.Identidade()));
    }

    /// <summary>As três quantidades sempre fecham entre si, qualquer que seja o momento da leitura.</summary>
    [Fact]
    public async Task O_catalogo_expoe_estoque_reservado_e_disponivel_coerentes()
    {
        var produtos = await _cliente.ListarProdutosAsync();

        produtos.ShouldAllBe(produto => produto.QuantidadeDisponivel == produto.QuantidadeEstoque - produto.QuantidadeReservada);
        produtos.ShouldAllBe(produto => produto.QuantidadeReservada >= 0 && produto.QuantidadeReservada <= produto.QuantidadeEstoque);
        // Nenhuma operação cria estoque: o físico nunca passa do que o catálogo semeou.
        produtos.ShouldAllBe(produto =>
            produto.QuantidadeEstoque <= CatalogoDeReferencia.Produtos.Single(seed => seed.Id == produto.Id).QuantidadeEstoque);
    }

    [Fact]
    public async Task Obtem_um_produto_expondo_preco_liquido_e_quantidade_disponivel()
    {
        var referencia = CatalogoDeReferencia.Produtos[0];

        var produto = await _cliente.GetFromJsonAsync<ProdutoResponse>(
            $"/api/produtos/{referencia.Id}", ClienteApi.Json, TestContext.Current.CancellationToken);

        produto.ShouldNotBeNull();
        produto.PrecoLiquido.ShouldBe(referencia.PrecoLiquido);
        produto.QuantidadeDisponivel.ShouldBe(produto.QuantidadeEstoque - produto.QuantidadeReservada);
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

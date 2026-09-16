using System.Net;
using System.Text;
using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Carrinhos;

namespace CarrinhoCompras.Api.IntegrationTests.Itens;

public sealed class ItensTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Adicionar_sem_quantidade_entra_com_1_e_expoe_preco_unitario_estoque_e_preco_do_item()
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(1);
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var atualizado = await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id);

        var item = atualizado.Itens.ShouldHaveSingleItem();
        item.ProdutoId.ShouldBe(produto.Id);
        item.DescricaoProduto.ShouldBe(produto.DescricaoProduto);
        item.Quantidade.ShouldBe(1);
        item.PrecoLiquidoUnitario.ShouldBe(produto.PrecoLiquido);
        item.QuantidadeEstoque.ShouldBe(produto.QuantidadeEstoque);
        item.PrecoItem.ShouldBe(produto.PrecoLiquido);
        atualizado.Subtotal.ShouldBe(produto.PrecoLiquido);
        atualizado.Total.ShouldBe(produto.PrecoLiquido);
    }

    [Fact]
    public async Task Adicionar_produto_novo_entra_com_a_quantidade_informada()
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(2);
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var atualizado = await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 2);

        var item = atualizado.Itens.ShouldHaveSingleItem();
        item.Quantidade.ShouldBe(2);
        item.PrecoItem.ShouldBe(produto.PrecoLiquido * 2);
    }

    [Fact]
    public async Task Adicionar_produto_que_ja_esta_no_carrinho_soma_a_quantidade()
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(3);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 1);

        var atualizado = await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 2);

        var item = atualizado.Itens.ShouldHaveSingleItem();
        item.Quantidade.ShouldBe(3);
        item.PrecoItem.ShouldBe(produto.PrecoLiquido * 3);
        (await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id)).ShouldBeEquivalentTo(atualizado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Adicionar_com_quantidade_menor_ou_igual_a_zero_retorna_400(int quantidade)
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var resposta = await _cliente.AdicionarItemAsync(carrinho.Id, Suporte.Catalogo.Produtos[0].Id, quantidade);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.BadRequest, "requisicao.invalida");
        problema.ErrosDoCampo("quantidade").ShouldBe(new[] { "A quantidade deve ser maior que zero." });
    }

    [Fact]
    public async Task Adicionar_alem_do_estoque_retorna_422_e_nao_altera_o_carrinho()
    {
        var produto = Suporte.Catalogo.ComMenorEstoquePositivo();
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, produto.QuantidadeEstoque);
        var antes = await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id);

        var resposta = await _cliente.AdicionarItemAsync(carrinho.Id, produto.Id, quantidade: 1);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.UnprocessableEntity, "produto.estoque_insuficiente");
        problema.Detalhe().ShouldContain(produto.DescricaoProduto);
        (await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id)).ShouldBeEquivalentTo(antes);
    }

    [Fact]
    public async Task Adicionar_produto_inexistente_retorna_404()
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var resposta = await _cliente.AdicionarItemAsync(carrinho.Id, Suporte.Catalogo.IdInexistente);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.NotFound, "produto.nao_encontrado");
    }

    [Fact]
    public async Task Adicionar_em_carrinho_inexistente_retorna_404()
    {
        var resposta = await _cliente.AdicionarItemAsync(Guid.CreateVersion7(), Suporte.Catalogo.Produtos[0].Id);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.NotFound, "carrinho.nao_encontrado");
    }

    [Fact]
    public async Task Alterar_quantidade_substitui_o_valor_e_recalcula_aumentando_e_diminuindo()
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(7);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 2);

        foreach (var quantidade in new[] { 5, 7, 1 })
        {
            var atualizado = await (await _cliente.AlterarQuantidadeAsync(carrinho.Id, produto.Id, quantidade))
                .DeveTerSucessoAsync<CarrinhoResponse>();

            var item = atualizado.Itens.ShouldHaveSingleItem();
            item.Quantidade.ShouldBe(quantidade);
            item.PrecoItem.ShouldBe(produto.PrecoLiquido * quantidade);
            atualizado.Subtotal.ShouldBe(produto.PrecoLiquido * quantidade);
            atualizado.Total.ShouldBe(atualizado.Subtotal - atualizado.Desconto);
        }
    }

    [Fact]
    public async Task Alterar_quantidade_de_produto_fora_do_carrinho_retorna_404()
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var resposta = await _cliente.AlterarQuantidadeAsync(carrinho.Id, Suporte.Catalogo.Produtos[0].Id, 2);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.NotFound, "carrinho.item_nao_encontrado");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Alterar_para_quantidade_menor_ou_igual_a_zero_retorna_400(int quantidade)
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(1);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id);

        var resposta = await _cliente.AlterarQuantidadeAsync(carrinho.Id, produto.Id, quantidade);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.BadRequest, "requisicao.invalida");
        problema.ErrosDoCampo("quantidade").ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Alterar_para_quantidade_acima_do_estoque_retorna_422()
    {
        var produto = Suporte.Catalogo.ComMenorEstoquePositivo();
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id);

        var resposta = await _cliente.AlterarQuantidadeAsync(carrinho.Id, produto.Id, produto.QuantidadeEstoque + 1);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.UnprocessableEntity, "produto.estoque_insuficiente");
    }

    [Fact]
    public async Task Remover_item_recalcula_subtotal_desconto_e_total()
    {
        var produtoA = Suporte.Catalogo.ComEstoqueDePeloMenos(2);
        var produtoB = Suporte.Catalogo.ComEstoqueDePeloMenos(1, diferenteDe: produtoA);
        var cupom = Suporte.Catalogo.Cupons[0];
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produtoA.Id, quantidade: 2);
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produtoB.Id);
        (await _cliente.AplicarCupomAsync(carrinho.Id, cupom.CodigoCupom)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var atualizado = await (await _cliente.RemoverItemAsync(carrinho.Id, produtoB.Id)).DeveTerSucessoAsync<CarrinhoResponse>();

        var subtotalEsperado = produtoA.PrecoLiquido * 2;
        atualizado.Itens.ShouldHaveSingleItem().ProdutoId.ShouldBe(produtoA.Id);
        atualizado.Subtotal.ShouldBe(subtotalEsperado);
        atualizado.Desconto.ShouldBe(Suporte.Catalogo.Desconto(subtotalEsperado, cupom));
        atualizado.Total.ShouldBe(subtotalEsperado - atualizado.Desconto);
    }

    [Fact]
    public async Task Remover_produto_que_nao_esta_no_carrinho_retorna_404()
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var resposta = await _cliente.RemoverItemAsync(carrinho.Id, Suporte.Catalogo.Produtos[0].Id);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.NotFound, "carrinho.item_nao_encontrado");
    }

    [Theory]
    [InlineData("""{"produtoId":""", "produtoId")]
    [InlineData("""{"produtoId":"1"}""", "produtoId")]
    [InlineData("""{"produtoId":1,"quantidade":"dois"}""", "quantidade")]
    [InlineData("", "corpo")]
    public async Task Corpo_malformado_ou_com_tipo_errado_retorna_400_indicando_o_campo(string corpo, string campo)
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();
        using var conteudo = new StringContent(corpo, Encoding.UTF8, "application/json");

        var resposta = await _cliente.PostAsync($"/api/carrinhos/{carrinho.Id}/itens", conteudo, TestContext.Current.CancellationToken);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.BadRequest, "requisicao.invalida");
        problema.ErrosDoCampo(campo).ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Corpo_que_nao_e_json_retorna_415()
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();
        using var conteudo = new StringContent("produtoId=1", Encoding.UTF8, "text/plain");

        var resposta = await _cliente.PostAsync($"/api/carrinhos/{carrinho.Id}/itens", conteudo, TestContext.Current.CancellationToken);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.UnsupportedMediaType, "midia.nao_suportada");
    }
}

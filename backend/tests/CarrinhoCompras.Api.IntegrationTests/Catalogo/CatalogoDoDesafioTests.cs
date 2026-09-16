using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Carrinhos;

namespace CarrinhoCompras.Api.IntegrationTests.Catalogo;

/// <summary>
/// Trava o que o enunciado pede do catálogo entregue: 10 produtos e os cupons 10OFF (10%) e 15OFF (15%).
/// Os demais testes escolhem produtos por característica (estoque mínimo, etc.) e sobrevivem a uma troca de
/// catálogo — estes aqui existem justamente para que essa troca não passe despercebida.
/// </summary>
public sealed class CatalogoDoDesafioTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task O_banco_tem_os_dez_produtos_do_enunciado()
    {
        var produtos = await _cliente.ListarProdutosAsync();

        produtos.Count.ShouldBe(10);
        produtos.Select(produto => produto.Id).ShouldBeUnique();
        produtos.ShouldAllBe(produto => !string.IsNullOrWhiteSpace(produto.DescricaoProduto));
        produtos.ShouldAllBe(produto => produto.PrecoLiquido > 0 && produto.QuantidadeEstoque >= 0);
    }

    [Theory]
    [InlineData("10OFF", 10)]
    [InlineData("15OFF", 15)]
    public async Task O_banco_tem_o_cupom_do_enunciado_com_o_percentual_certo(string codigoCupom, decimal percentual)
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var comCupom = await (await _cliente.AplicarCupomAsync(carrinho.Id, codigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();

        comCupom.Cupom.ShouldNotBeNull();
        comCupom.Cupom.CodigoCupom.ShouldBe(codigoCupom);
        comCupom.Cupom.PercentualDesconto.ShouldBe(percentual);
    }
}

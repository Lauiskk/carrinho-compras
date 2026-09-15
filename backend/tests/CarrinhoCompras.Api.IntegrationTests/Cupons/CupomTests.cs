using System.Net;
using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Carrinhos;

namespace CarrinhoCompras.Api.IntegrationTests.Cupons;

public sealed class CupomTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Aplicar_cupom_calcula_desconto_sobre_o_subtotal()
    {
        var (carrinho, subtotal) = await CriarCarrinhoComItensAsync();
        var cupom = Suporte.Catalogo.Cupons[0];

        var atualizado = await (await _cliente.AplicarCupomAsync(carrinho.Id, cupom.CodigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();

        atualizado.Cupom.ShouldBe(new CupomAplicadoResponse(cupom.CodigoCupom, cupom.PercentualDesconto));
        atualizado.Subtotal.ShouldBe(subtotal);
        atualizado.Desconto.ShouldBe(Suporte.Catalogo.Desconto(subtotal, cupom));
        atualizado.Total.ShouldBe(subtotal - atualizado.Desconto);
    }

    [Fact]
    public async Task Aplicar_outro_cupom_substitui_o_anterior_e_so_o_ultimo_fica_ativo()
    {
        var (carrinho, subtotal) = await CriarCarrinhoComItensAsync();
        var primeiro = Suporte.Catalogo.Cupons[0];
        var segundo = Suporte.Catalogo.Cupons[1];

        await (await _cliente.AplicarCupomAsync(carrinho.Id, primeiro.CodigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();
        await (await _cliente.AplicarCupomAsync(carrinho.Id, segundo.CodigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();

        var consultado = await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id);
        consultado.Cupom.ShouldBe(new CupomAplicadoResponse(segundo.CodigoCupom, segundo.PercentualDesconto));
        consultado.Desconto.ShouldBe(Suporte.Catalogo.Desconto(subtotal, segundo));
        consultado.Total.ShouldBe(subtotal - consultado.Desconto);
    }

    [Fact]
    public async Task Codigo_do_cupom_ignora_espacos_e_maiusculas()
    {
        var (carrinho, _) = await CriarCarrinhoComItensAsync();
        var cupom = Suporte.Catalogo.Cupons[0];

        var atualizado = await (await _cliente.AplicarCupomAsync(carrinho.Id, $"  {cupom.CodigoCupom.ToLowerInvariant()} "))
            .DeveTerSucessoAsync<CarrinhoResponse>();

        atualizado.Cupom.ShouldNotBeNull().CodigoCupom.ShouldBe(cupom.CodigoCupom);
    }

    [Fact]
    public async Task Cupom_inexistente_retorna_422_e_mantem_o_cupom_anterior()
    {
        var (carrinho, _) = await CriarCarrinhoComItensAsync();
        var cupomValido = Suporte.Catalogo.Cupons[0];
        await (await _cliente.AplicarCupomAsync(carrinho.Id, cupomValido.CodigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();

        var resposta = await _cliente.AplicarCupomAsync(carrinho.Id, "NAOEXISTE");

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.UnprocessableEntity, "cupom.invalido");
        problema.Detalhe().ShouldContain("NAOEXISTE");
        (await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id)).Cupom.ShouldNotBeNull().CodigoCupom.ShouldBe(cupomValido.CodigoCupom);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Codigo_de_cupom_vazio_retorna_400(string? codigo)
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var resposta = await _cliente.AplicarCupomAsync(carrinho.Id, codigo);

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.BadRequest, "requisicao.invalida");
        problema.ErrosDoCampo("codigoCupom").ShouldBe(new[] { "Informe o código do cupom." });
    }

    [Fact]
    public async Task Remover_cupom_zera_o_desconto_e_pode_ser_repetido()
    {
        var (carrinho, subtotal) = await CriarCarrinhoComItensAsync();
        await (await _cliente.AplicarCupomAsync(carrinho.Id, Suporte.Catalogo.Cupons[0].CodigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();

        var semCupom = await (await _cliente.RemoverCupomAsync(carrinho.Id)).DeveTerSucessoAsync<CarrinhoResponse>();
        var deNovo = await (await _cliente.RemoverCupomAsync(carrinho.Id)).DeveTerSucessoAsync<CarrinhoResponse>();

        semCupom.Cupom.ShouldBeNull();
        semCupom.Desconto.ShouldBe(0m);
        semCupom.Total.ShouldBe(subtotal);
        deNovo.ShouldBeEquivalentTo(semCupom);
    }

    [Fact]
    public async Task Cupom_em_carrinho_vazio_passa_a_descontar_quando_itens_entram()
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();
        var cupom = Suporte.Catalogo.Cupons[0];
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(2);

        var vazio = await (await _cliente.AplicarCupomAsync(carrinho.Id, cupom.CodigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();
        var comItens = await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 2);

        vazio.Desconto.ShouldBe(0m);
        vazio.Total.ShouldBe(0m);
        comItens.Desconto.ShouldBe(Suporte.Catalogo.Desconto(produto.PrecoLiquido * 2, cupom));
    }

    private async Task<(CarrinhoResponse Carrinho, decimal Subtotal)> CriarCarrinhoComItensAsync()
    {
        var produtoA = Suporte.Catalogo.ComEstoqueDePeloMenos(3);
        var produtoB = Suporte.Catalogo.ComEstoqueDePeloMenos(1, diferenteDe: produtoA);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produtoA.Id, quantidade: 3);
        var atualizado = await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produtoB.Id);
        return (atualizado, (produtoA.PrecoLiquido * 3) + produtoB.PrecoLiquido);
    }
}

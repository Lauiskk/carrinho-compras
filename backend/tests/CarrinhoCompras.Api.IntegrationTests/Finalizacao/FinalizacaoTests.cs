using System.Net;
using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Carrinhos;
using CarrinhoCompras.Domain.Carrinhos;

namespace CarrinhoCompras.Api.IntegrationTests.Finalizacao;

public sealed class FinalizacaoTests(ApiFactory api)
{
    private const string MensagemFinalizado =
        "Este carrinho já foi finalizado e não pode mais ser alterado. Crie um novo carrinho para continuar comprando.";

    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Finalizar_muda_o_status_registra_a_data_e_preserva_os_valores()
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(2);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 2);
        var antes = await (await _cliente.AplicarCupomAsync(carrinho.Id, Suporte.Catalogo.Cupons[0].CodigoCupom))
            .DeveTerSucessoAsync<CarrinhoResponse>();

        var finalizado = await (await _cliente.FinalizarAsync(carrinho.Id)).DeveTerSucessoAsync<CarrinhoResponse>();

        finalizado.Status.ShouldBe(StatusCarrinho.Finalizado);
        finalizado.FinalizadoEm.ShouldNotBeNull();
        finalizado.Subtotal.ShouldBe(antes.Subtotal);
        finalizado.Desconto.ShouldBe(antes.Desconto);
        finalizado.Total.ShouldBe(antes.Total);
        (await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id)).Status.ShouldBe(StatusCarrinho.Finalizado);
    }

    [Fact]
    public async Task Finalizar_carrinho_vazio_retorna_422()
    {
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var resposta = await _cliente.FinalizarAsync(carrinho.Id);

        await resposta.DeveSerProblemaAsync(HttpStatusCode.UnprocessableEntity, "carrinho.vazio");
    }

    public static TheoryData<string> Alteracoes =>
    [
        "adicionar item",
        "adicionar produto inexistente",
        "alterar quantidade",
        "remover item",
        "aplicar cupom",
        "aplicar cupom inexistente",
        "remover cupom",
        "finalizar de novo",
    ];

    [Theory]
    [MemberData(nameof(Alteracoes))]
    public async Task Carrinho_finalizado_rejeita_qualquer_alteracao_com_409_e_mensagem_clara(string alteracao)
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(2);
        var outroProduto = Suporte.Catalogo.ComEstoqueDePeloMenos(1, diferenteDe: produto);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id);
        await (await _cliente.AplicarCupomAsync(carrinho.Id, Suporte.Catalogo.Cupons[0].CodigoCupom)).DeveTerSucessoAsync<CarrinhoResponse>();
        var finalizado = await (await _cliente.FinalizarAsync(carrinho.Id)).DeveTerSucessoAsync<CarrinhoResponse>();

        var resposta = alteracao switch
        {
            "adicionar item" => await _cliente.AdicionarItemAsync(carrinho.Id, outroProduto.Id),
            "adicionar produto inexistente" => await _cliente.AdicionarItemAsync(carrinho.Id, Suporte.Catalogo.IdInexistente),
            "alterar quantidade" => await _cliente.AlterarQuantidadeAsync(carrinho.Id, produto.Id, 2),
            "remover item" => await _cliente.RemoverItemAsync(carrinho.Id, produto.Id),
            "aplicar cupom" => await _cliente.AplicarCupomAsync(carrinho.Id, Suporte.Catalogo.Cupons[1].CodigoCupom),
            "aplicar cupom inexistente" => await _cliente.AplicarCupomAsync(carrinho.Id, "NAOEXISTE"),
            "remover cupom" => await _cliente.RemoverCupomAsync(carrinho.Id),
            "finalizar de novo" => await _cliente.FinalizarAsync(carrinho.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(alteracao), alteracao, "Alteração desconhecida."),
        };

        var problema = await resposta.DeveSerProblemaAsync(HttpStatusCode.Conflict, "carrinho.finalizado");
        problema.Detalhe().ShouldBe(MensagemFinalizado);
        (await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id)).ShouldBeEquivalentTo(finalizado);
    }
}

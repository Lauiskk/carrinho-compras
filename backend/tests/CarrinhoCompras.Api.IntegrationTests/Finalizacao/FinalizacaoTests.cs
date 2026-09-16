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
        var produto = await _cliente.ProdutoComDisponivelAsync(2);
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

    /// <summary>
    /// O ciclo completo do estoque: entrar na sacola prende unidades (o disponível cai, o físico não),
    /// e o checkout transforma a reserva em venda (o físico cai e a reserva é devolvida).
    /// </summary>
    [Fact]
    public async Task Finalizar_transforma_a_reserva_em_baixa_de_estoque()
    {
        var produto = await _cliente.ProdutoComDisponivelAsync(2);
        var carrinho = await _cliente.CriarCarrinhoAsync();

        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 2);

        // Na sacola: o disponível caiu, mas a loja ainda tem as peças.
        var reservado = await _cliente.ProdutoAtualAsync(produto.Id);
        reservado.QuantidadeEstoque.ShouldBe(produto.QuantidadeEstoque);
        reservado.QuantidadeReservada.ShouldBe(produto.QuantidadeReservada + 2);
        reservado.QuantidadeDisponivel.ShouldBe(produto.QuantidadeDisponivel - 2);

        (await _cliente.FinalizarAsync(carrinho.Id)).StatusCode.ShouldBe(HttpStatusCode.OK);

        // Vendido: saiu do estoque físico e deixou de estar reservado; o disponível não volta.
        var vendido = await _cliente.ProdutoAtualAsync(produto.Id);
        vendido.QuantidadeEstoque.ShouldBe(produto.QuantidadeEstoque - 2);
        vendido.QuantidadeReservada.ShouldBe(produto.QuantidadeReservada);
        vendido.QuantidadeDisponivel.ShouldBe(produto.QuantidadeDisponivel - 2);
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
        var produto = await _cliente.ProdutoComDisponivelAsync(2);
        var outroProduto = await _cliente.ProdutoComDisponivelAsync(1, produto.Id);
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

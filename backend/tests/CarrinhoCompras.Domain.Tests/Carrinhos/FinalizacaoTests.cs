using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Tests.Suporte;

namespace CarrinhoCompras.Domain.Tests.Carrinhos;

/// <summary>Checkout: status, carrinho vazio e bloqueio de qualquer alteração após finalizar.</summary>
public sealed class FinalizacaoTests
{
    [Fact]
    public void Finalizar_muda_o_status_registra_a_data_e_preserva_os_totais()
    {
        var carrinho = Dados.CarrinhoCom((Dados.Produto(preco: 10.50m), 2));
        carrinho.AplicarCupom(Dados.Cupom(10m)).DeveTerSucesso();
        var momento = Dados.Agora.AddMinutes(5); // dentro da janela de reserva

        carrinho.Finalizar(momento).DeveTerSucesso();

        carrinho.Status.ShouldBe(StatusCarrinho.Finalizado);
        carrinho.EstaFinalizado.ShouldBeTrue();
        carrinho.FinalizadoEm.ShouldBe(momento);
        carrinho.Subtotal.ShouldBe(21.00m);
        carrinho.Desconto.ShouldBe(2.10m);
        carrinho.Total.ShouldBe(18.90m);
    }

    [Fact]
    public void Carrinho_aberto_pode_ser_alterado_e_finalizado_nao()
    {
        var carrinho = Dados.CarrinhoCom((Dados.Produto(), 1));
        carrinho.VerificarSePodeSerAlterado().DeveTerSucesso();

        carrinho.Finalizar(Dados.Agora).DeveTerSucesso();

        carrinho.VerificarSePodeSerAlterado().DeveFalharCom(CarrinhoErros.Finalizado);
    }

    [Fact]
    public void Finalizar_carrinho_vazio_falha()
    {
        var carrinho = Dados.CarrinhoNovo();

        var resultado = carrinho.Finalizar(Dados.Agora);

        resultado.DeveFalharCom(CarrinhoErros.Vazio);
        resultado.Error!.Type.ShouldBe(ErrorType.BusinessRule);
        carrinho.Status.ShouldBe(StatusCarrinho.Aberto);
    }

    public static TheoryData<string> OperacoesDeAlteracao =>
    [
        nameof(Carrinho.AdicionarItem),
        nameof(Carrinho.AlterarQuantidadeItem),
        nameof(Carrinho.RemoverItem),
        nameof(Carrinho.AplicarCupom),
        nameof(Carrinho.RemoverCupom),
        nameof(Carrinho.Finalizar),
    ];

    [Theory]
    [MemberData(nameof(OperacoesDeAlteracao))]
    public void Carrinho_finalizado_rejeita_qualquer_alteracao_com_erro_claro_e_estado_intacto(string operacao)
    {
        var produto = Dados.Produto(id: 1, estoque: 10);
        var outroProduto = Dados.Produto(id: 2, estoque: 10);
        var carrinho = Dados.CarrinhoCom((produto, 2));
        carrinho.AplicarCupom(Dados.Cupom(10m, id: 1)).DeveTerSucesso();
        carrinho.Finalizar(Dados.Agora).DeveTerSucesso();
        var antes = carrinho.Retrato();

        var resultado = operacao switch
        {
            nameof(Carrinho.AdicionarItem) => carrinho.AdicionarItem(outroProduto, 1),
            nameof(Carrinho.AlterarQuantidadeItem) => carrinho.AlterarQuantidadeItem(produto.Id, 3),
            nameof(Carrinho.RemoverItem) => carrinho.RemoverItem(produto.Id),
            nameof(Carrinho.AplicarCupom) => carrinho.AplicarCupom(Dados.Cupom(15m, id: 2)),
            nameof(Carrinho.RemoverCupom) => carrinho.RemoverCupom(),
            nameof(Carrinho.Finalizar) => carrinho.Finalizar(Dados.Agora.AddHours(1)),
            _ => throw new ArgumentOutOfRangeException(nameof(operacao), operacao, "Operação desconhecida."),
        };

        resultado.DeveFalharCom(CarrinhoErros.Finalizado);
        resultado.Error!.Type.ShouldBe(ErrorType.Conflict);
        resultado.Error.Message.ShouldBe(
            "Este carrinho já foi finalizado e não pode mais ser alterado. Crie um novo carrinho para continuar comprando.");
        carrinho.Retrato().ShouldBe(antes);
        carrinho.FinalizadoEm.ShouldBe(Dados.Agora);
    }
}

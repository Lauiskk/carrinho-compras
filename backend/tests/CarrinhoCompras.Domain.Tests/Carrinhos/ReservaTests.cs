using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Tests.Suporte;

namespace CarrinhoCompras.Domain.Tests.Carrinhos;

/// <summary>
/// Reserva de estoque: o que entra na sacola sai da vitrine, volta quando sai da sacola ou quando o prazo
/// vence, e vira baixa definitiva no checkout.
/// </summary>
public sealed class ReservaTests
{
    [Fact]
    public void Adicionar_prende_as_unidades_sem_tirar_do_estoque_fisico()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoNovo();

        carrinho.AdicionarItem(produto, 2).DeveTerSucesso();

        produto.QuantidadeEstoque.ShouldBe(5);
        produto.QuantidadeReservada.ShouldBe(2);
        produto.QuantidadeDisponivel.ShouldBe(3);
    }

    [Fact]
    public void Remover_item_devolve_as_unidades_para_a_vitrine()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 3));

        carrinho.RemoverItem(produto.Id).DeveTerSucesso();

        produto.QuantidadeReservada.ShouldBe(0);
        produto.QuantidadeDisponivel.ShouldBe(5);
    }

    [Theory]
    [InlineData(2, 5, 5, 0)] // aumentar prende mais
    [InlineData(4, 1, 1, 4)] // diminuir devolve a diferença
    public void Alterar_quantidade_ajusta_apenas_a_diferenca(int inicial, int nova, int reservadaEsperada, int disponivelEsperado)
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, inicial));

        carrinho.AlterarQuantidadeItem(produto.Id, nova).DeveTerSucesso();

        produto.QuantidadeReservada.ShouldBe(reservadaEsperada);
        produto.QuantidadeDisponivel.ShouldBe(disponivelEsperado);
    }

    [Fact]
    public void Finalizar_transforma_a_reserva_em_baixa_de_estoque()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 2));

        carrinho.Finalizar(Dados.Agora).DeveTerSucesso();

        produto.QuantidadeEstoque.ShouldBe(3);
        produto.QuantidadeReservada.ShouldBe(0);
        produto.QuantidadeDisponivel.ShouldBe(3);
    }

    /// <summary>
    /// O ponto que a reserva existe para resolver: sem ela, duas sacolas levariam a mesma última unidade
    /// e as duas conseguiriam finalizar.
    /// </summary>
    [Fact]
    public void Duas_sacolas_nao_levam_a_mesma_ultima_unidade()
    {
        var produto = Dados.Produto(estoque: 1);
        var primeira = Dados.CarrinhoCom((produto, 1));
        var segunda = Dados.CarrinhoNovo();

        segunda.AdicionarItem(produto, 1).DeveFalharComCodigo("produto.estoque_insuficiente", ErrorType.BusinessRule);

        primeira.Finalizar(Dados.Agora).DeveTerSucesso();
        produto.QuantidadeEstoque.ShouldBe(0);
        segunda.Itens.ShouldBeEmpty();
    }

    [Fact]
    public void Sacola_parada_alem_do_prazo_devolve_as_unidades_e_encerra()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 2));
        carrinho.ExpiraEm.ShouldBe(Dados.Agora + Carrinho.JanelaDeReserva);

        carrinho.ExpirarSeVencido(Dados.Agora + Carrinho.JanelaDeReserva + TimeSpan.FromSeconds(1)).ShouldBeTrue();

        carrinho.Status.ShouldBe(StatusCarrinho.Expirado);
        carrinho.EstaExpirado.ShouldBeTrue();
        carrinho.ExpiraEm.ShouldBeNull();
        produto.QuantidadeReservada.ShouldBe(0);
        produto.QuantidadeDisponivel.ShouldBe(5);
    }

    [Fact]
    public void Expirar_e_idempotente_e_nao_devolve_duas_vezes()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 2));
        var depoisDoPrazo = Dados.Agora + Carrinho.JanelaDeReserva + TimeSpan.FromMinutes(1);

        carrinho.ExpirarSeVencido(depoisDoPrazo).ShouldBeTrue();
        carrinho.ExpirarSeVencido(depoisDoPrazo).ShouldBeFalse();

        produto.QuantidadeReservada.ShouldBe(0);
    }

    [Fact]
    public void Mexer_na_sacola_renova_o_prazo()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 1));
        var maisTarde = Dados.Agora.AddMinutes(10);

        carrinho.AdicionarItem(produto, 1, maisTarde).DeveTerSucesso();

        carrinho.ExpiraEm.ShouldBe(maisTarde + Carrinho.JanelaDeReserva);
    }

    [Fact]
    public void Sacola_vazia_nao_tem_prazo_porque_nao_segura_nada()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 1));

        carrinho.RemoverItem(produto.Id).DeveTerSucesso();

        carrinho.ExpiraEm.ShouldBeNull();
        carrinho.ExpirarSeVencido(Dados.Agora.AddDays(1)).ShouldBeFalse();
        carrinho.Status.ShouldBe(StatusCarrinho.Aberto);
    }

    [Fact]
    public void Compra_finalizada_nao_expira()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 2));
        carrinho.Finalizar(Dados.Agora).DeveTerSucesso();

        carrinho.ExpirarSeVencido(Dados.Agora.AddDays(1)).ShouldBeFalse();

        carrinho.Status.ShouldBe(StatusCarrinho.Finalizado);
        produto.QuantidadeEstoque.ShouldBe(3); // a venda não é desfeita
    }

    public static TheoryData<string> Alteracoes =>
    [
        "adicionar item",
        "alterar quantidade",
        "remover item",
        "aplicar cupom",
        "remover cupom",
        "finalizar",
    ];

    [Theory]
    [MemberData(nameof(Alteracoes))]
    public void Sacola_expirada_rejeita_qualquer_alteracao(string alteracao)
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 1));
        var depoisDoPrazo = Dados.Agora + Carrinho.JanelaDeReserva + TimeSpan.FromMinutes(1);

        var resultado = alteracao switch
        {
            "adicionar item" => carrinho.AdicionarItem(produto, 1, depoisDoPrazo),
            "alterar quantidade" => carrinho.AlterarQuantidadeItem(produto.Id, 2, depoisDoPrazo),
            "remover item" => carrinho.RemoverItem(produto.Id, depoisDoPrazo),
            "aplicar cupom" => carrinho.AplicarCupom(Dados.Cupom(10m), depoisDoPrazo),
            "remover cupom" => carrinho.RemoverCupom(depoisDoPrazo),
            "finalizar" => carrinho.Finalizar(depoisDoPrazo),
            _ => throw new ArgumentOutOfRangeException(nameof(alteracao), alteracao, "Alteração desconhecida."),
        };

        resultado.DeveFalharCom(CarrinhoErros.Expirado);
        carrinho.Status.ShouldBe(StatusCarrinho.Expirado);
        // A tentativa já encontrou a sacola vencida: as unidades voltaram antes de a operação ser recusada.
        produto.QuantidadeDisponivel.ShouldBe(5);
    }
}

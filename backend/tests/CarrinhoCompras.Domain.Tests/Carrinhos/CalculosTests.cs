using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Produtos;
using CarrinhoCompras.Domain.Tests.Suporte;

namespace CarrinhoCompras.Domain.Tests.Carrinhos;

/// <summary>
/// Critério "Cálculos" do desafio: subtotal, desconto e total em carrinho vazio, com e sem cupom e com quantidades alteradas.
/// </summary>
public sealed class CalculosTests
{
    private static readonly Produto ProdutoA = Dados.Produto(id: 1, preco: 10.50m);
    private static readonly Produto ProdutoB = Dados.Produto(id: 2, preco: 33.33m);

    [Fact]
    public void Carrinho_novo_esta_aberto_vazio_e_com_totais_zerados()
    {
        var carrinho = Dados.CarrinhoNovo();

        carrinho.Status.ShouldBe(StatusCarrinho.Aberto);
        carrinho.Itens.ShouldBeEmpty();
        carrinho.Cupom.ShouldBeNull();
        carrinho.Subtotal.ShouldBe(0m);
        carrinho.Desconto.ShouldBe(0m);
        carrinho.Total.ShouldBe(0m);
        carrinho.CriadoEm.ShouldBe(Dados.Agora);
        carrinho.FinalizadoEm.ShouldBeNull();
        carrinho.Id.Version.ShouldBe(7);
    }

    [Fact]
    public void Cupom_em_carrinho_vazio_fica_aplicado_com_totais_zerados()
    {
        var carrinho = Dados.CarrinhoNovo();
        var cupom = Dados.Cupom(10m);

        carrinho.AplicarCupom(cupom).DeveTerSucesso();

        carrinho.Cupom.ShouldBe(cupom);
        carrinho.Subtotal.ShouldBe(0m);
        carrinho.Desconto.ShouldBe(0m);
        carrinho.Total.ShouldBe(0m);
    }

    [Fact]
    public void Cupom_aplicado_em_carrinho_vazio_passa_a_descontar_quando_itens_entram()
    {
        var carrinho = Dados.CarrinhoNovo();
        carrinho.AplicarCupom(Dados.Cupom(10m)).DeveTerSucesso();

        carrinho.AdicionarItem(ProdutoA, 2).DeveTerSucesso();

        carrinho.Subtotal.ShouldBe(21.00m);
        carrinho.Desconto.ShouldBe(2.10m);
        carrinho.Total.ShouldBe(18.90m);
    }

    [Fact]
    public void Sem_cupom_subtotal_e_a_soma_dos_itens_e_total_igual_ao_subtotal()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoA, 2), (ProdutoB, 3));

        carrinho.Itens.Single(i => i.ProdutoId == ProdutoA.Id).PrecoItem.ShouldBe(21.00m);
        carrinho.Itens.Single(i => i.ProdutoId == ProdutoB.Id).PrecoItem.ShouldBe(99.99m);
        carrinho.Subtotal.ShouldBe(120.99m);
        carrinho.Desconto.ShouldBe(0m);
        carrinho.Total.ShouldBe(120.99m);
    }

    [Theory]
    [InlineData(10, 12.10, 108.89)] // 120,99 × 10% = 12,099
    [InlineData(15, 18.15, 102.84)] // 120,99 × 15% = 18,1485
    public void Com_cupom_desconto_incide_sobre_o_subtotal_e_total_e_subtotal_menos_desconto(
        decimal percentual, decimal descontoEsperado, decimal totalEsperado)
    {
        var carrinho = Dados.CarrinhoCom((ProdutoA, 2), (ProdutoB, 3));

        carrinho.AplicarCupom(Dados.Cupom(percentual)).DeveTerSucesso();

        carrinho.Subtotal.ShouldBe(120.99m);
        carrinho.Desconto.ShouldBe(descontoEsperado);
        carrinho.Total.ShouldBe(totalEsperado);
    }

    [Theory]
    [InlineData(12.25, 10, 1.23, 11.02)] // 1,225 → 1,23 (arredondamento bancário daria 1,22)
    [InlineData(16.70, 15, 2.51, 14.19)] // 2,505 → 2,51
    [InlineData(33.33, 15, 5.00, 28.33)] // 4,9995 → 5,00
    public void Desconto_arredonda_para_duas_casas_com_meio_para_longe_de_zero(
        decimal preco, decimal percentual, decimal descontoEsperado, decimal totalEsperado)
    {
        var carrinho = Dados.CarrinhoCom((Dados.Produto(preco: preco), 1));

        carrinho.AplicarCupom(Dados.Cupom(percentual)).DeveTerSucesso();

        carrinho.Desconto.ShouldBe(descontoEsperado);
        carrinho.Total.ShouldBe(totalEsperado);
    }

    [Fact]
    public void Alterar_quantidade_recalcula_preco_do_item_e_totais_aumentando_e_diminuindo()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoB, 2));
        var item = carrinho.Itens.Single();
        item.PrecoItem.ShouldBe(66.66m);

        carrinho.AlterarQuantidadeItem(ProdutoB.Id, 5).DeveTerSucesso();
        item.PrecoItem.ShouldBe(166.65m);
        carrinho.Total.ShouldBe(166.65m);

        carrinho.AlterarQuantidadeItem(ProdutoB.Id, 7).DeveTerSucesso();
        item.PrecoItem.ShouldBe(233.31m);
        carrinho.Total.ShouldBe(233.31m);

        carrinho.AlterarQuantidadeItem(ProdutoB.Id, 1).DeveTerSucesso();
        item.PrecoItem.ShouldBe(33.33m);
        carrinho.Subtotal.ShouldBe(33.33m);
        carrinho.Total.ShouldBe(33.33m);
    }

    [Fact]
    public void Alterar_quantidade_com_cupom_recalcula_o_desconto()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoA, 2));
        carrinho.AplicarCupom(Dados.Cupom(10m)).DeveTerSucesso();

        carrinho.AlterarQuantidadeItem(ProdutoA.Id, 4).DeveTerSucesso();

        carrinho.Subtotal.ShouldBe(42.00m);
        carrinho.Desconto.ShouldBe(4.20m);
        carrinho.Total.ShouldBe(37.80m);
    }

    [Fact]
    public void Remover_item_recalcula_subtotal_desconto_e_total()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoA, 2), (ProdutoB, 3));
        carrinho.AplicarCupom(Dados.Cupom(10m)).DeveTerSucesso();

        carrinho.RemoverItem(ProdutoB.Id).DeveTerSucesso();

        carrinho.Itens.ShouldHaveSingleItem().ProdutoId.ShouldBe(ProdutoA.Id);
        carrinho.Subtotal.ShouldBe(21.00m);
        carrinho.Desconto.ShouldBe(2.10m);
        carrinho.Total.ShouldBe(18.90m);
    }

    [Fact]
    public void Remover_o_ultimo_item_zera_os_totais_e_mantem_o_cupom()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoA, 1));
        carrinho.AplicarCupom(Dados.Cupom(15m)).DeveTerSucesso();

        carrinho.RemoverItem(ProdutoA.Id).DeveTerSucesso();

        carrinho.Itens.ShouldBeEmpty();
        carrinho.Cupom.ShouldNotBeNull();
        carrinho.Subtotal.ShouldBe(0m);
        carrinho.Desconto.ShouldBe(0m);
        carrinho.Total.ShouldBe(0m);
    }

    [Fact]
    public void Aplicar_outro_cupom_substitui_o_anterior_e_so_o_ultimo_fica_ativo()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoA, 2), (ProdutoB, 3));
        var dezOff = Dados.Cupom(10m, id: 1, codigo: "10OFF");
        var quinzeOff = Dados.Cupom(15m, id: 2, codigo: "15OFF");

        carrinho.AplicarCupom(dezOff).DeveTerSucesso();
        carrinho.AplicarCupom(quinzeOff).DeveTerSucesso();

        carrinho.Cupom.ShouldBe(quinzeOff);
        carrinho.CupomId.ShouldBe(quinzeOff.Id);
        carrinho.Desconto.ShouldBe(18.15m);
        carrinho.Total.ShouldBe(102.84m);
    }

    [Fact]
    public void Remover_cupom_zera_o_desconto_e_repetir_a_remocao_nao_falha()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoA, 2));
        carrinho.AplicarCupom(Dados.Cupom(10m)).DeveTerSucesso();

        carrinho.RemoverCupom().DeveTerSucesso();
        carrinho.RemoverCupom().DeveTerSucesso();

        carrinho.Cupom.ShouldBeNull();
        carrinho.CupomId.ShouldBeNull();
        carrinho.Desconto.ShouldBe(0m);
        carrinho.Total.ShouldBe(carrinho.Subtotal);
    }

    [Fact]
    public void Cupom_de_cem_por_cento_zera_o_total()
    {
        var carrinho = Dados.CarrinhoCom((ProdutoB, 3));

        carrinho.AplicarCupom(Dados.Cupom(100m)).DeveTerSucesso();

        carrinho.Desconto.ShouldBe(99.99m);
        carrinho.Total.ShouldBe(0m);
    }
}

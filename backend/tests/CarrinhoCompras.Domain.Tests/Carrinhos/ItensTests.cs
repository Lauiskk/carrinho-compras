using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Tests.Suporte;

namespace CarrinhoCompras.Domain.Tests.Carrinhos;

/// <summary>Regras de itens do carrinho: adicionar, alterar quantidade, remover e limite de estoque.</summary>
public sealed class ItensTests
{
    [Fact]
    public void Adicionar_produto_novo_com_quantidade_1_cria_uma_linha_com_o_preco_do_catalogo()
    {
        var produto = Dados.Produto(preco: 25.00m);
        var carrinho = Dados.CarrinhoNovo();

        carrinho.AdicionarItem(produto, 1).DeveTerSucesso();

        var item = carrinho.Itens.ShouldHaveSingleItem();
        item.ProdutoId.ShouldBe(produto.Id);
        item.Produto.ShouldBe(produto);
        item.Quantidade.ShouldBe(1);
        item.PrecoUnitario.ShouldBe(25.00m);
        item.PrecoItem.ShouldBe(25.00m);
        item.CarrinhoId.ShouldBe(carrinho.Id);
    }

    [Fact]
    public void Adicionar_produto_novo_entra_com_a_quantidade_informada()
    {
        var carrinho = Dados.CarrinhoNovo();

        carrinho.AdicionarItem(Dados.Produto(preco: 10.50m), 3).DeveTerSucesso();

        var item = carrinho.Itens.ShouldHaveSingleItem();
        item.Quantidade.ShouldBe(3);
        item.PrecoItem.ShouldBe(31.50m);
    }

    [Fact]
    public void Adicionar_produto_que_ja_esta_no_carrinho_soma_a_quantidade_sem_duplicar_a_linha()
    {
        var produto = Dados.Produto(preco: 10.50m);
        var carrinho = Dados.CarrinhoNovo();

        carrinho.AdicionarItem(produto, 1).DeveTerSucesso();
        carrinho.AdicionarItem(produto, 1).DeveTerSucesso();
        carrinho.Itens.ShouldHaveSingleItem().Quantidade.ShouldBe(2);

        carrinho.AdicionarItem(produto, 3).DeveTerSucesso();

        var item = carrinho.Itens.ShouldHaveSingleItem();
        item.Quantidade.ShouldBe(5);
        item.PrecoItem.ShouldBe(52.50m);
        carrinho.Subtotal.ShouldBe(52.50m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Adicionar_com_quantidade_menor_ou_igual_a_zero_falha_sem_alterar_o_carrinho(int quantidade)
    {
        var produto = Dados.Produto();
        var carrinho = Dados.CarrinhoCom((produto, 1));
        var antes = carrinho.Retrato();

        carrinho.AdicionarItem(produto, quantidade).DeveFalharCom(CarrinhoErros.QuantidadeInvalida);

        carrinho.Retrato().ShouldBe(antes);
        CarrinhoErros.QuantidadeInvalida.Type.ShouldBe(ErrorType.Validation);
    }

    [Fact]
    public void Adicionar_acima_do_estoque_falha_com_mensagem_clara_sem_alterar_o_carrinho()
    {
        var produto = Dados.Produto(estoque: 2, descricao: "Botas de Passos Silenciosos");
        var carrinho = Dados.CarrinhoCom((produto, 2));
        var antes = carrinho.Retrato();

        var resultado = carrinho.AdicionarItem(produto, 1);

        resultado.DeveFalharComCodigo("produto.estoque_insuficiente", ErrorType.BusinessRule);
        resultado.Error!.Message.ShouldBe(
            "Estoque insuficiente para 'Botas de Passos Silenciosos': o carrinho ficaria com 3 unidade(s), mas há apenas 2 em estoque.");
        carrinho.Retrato().ShouldBe(antes);
    }

    [Fact]
    public void Adicionar_produto_novo_acima_do_estoque_falha()
    {
        var carrinho = Dados.CarrinhoNovo();

        carrinho.AdicionarItem(Dados.Produto(estoque: 2), 3)
            .DeveFalharComCodigo("produto.estoque_insuficiente", ErrorType.BusinessRule);

        carrinho.Itens.ShouldBeEmpty();
    }

    [Fact]
    public void Adicionar_ate_exatamente_o_estoque_disponivel_e_permitido()
    {
        var produto = Dados.Produto(estoque: 5);
        var carrinho = Dados.CarrinhoCom((produto, 3));

        carrinho.AdicionarItem(produto, 2).DeveTerSucesso();

        carrinho.Itens.ShouldHaveSingleItem().Quantidade.ShouldBe(5);
    }

    [Fact]
    public void Produto_sem_estoque_nao_pode_ser_adicionado()
    {
        var carrinho = Dados.CarrinhoNovo();

        carrinho.AdicionarItem(Dados.Produto(estoque: 0), 1)
            .DeveFalharComCodigo("produto.estoque_insuficiente", ErrorType.BusinessRule);
    }

    [Fact]
    public void Somar_quantidade_enorme_nao_estoura_o_inteiro_e_respeita_o_estoque()
    {
        var produto = Dados.Produto(estoque: 10);
        var carrinho = Dados.CarrinhoCom((produto, 5));

        carrinho.AdicionarItem(produto, int.MaxValue)
            .DeveFalharComCodigo("produto.estoque_insuficiente", ErrorType.BusinessRule);

        carrinho.Itens.ShouldHaveSingleItem().Quantidade.ShouldBe(5);
    }

    [Fact]
    public void Alterar_quantidade_de_produto_fora_do_carrinho_falha_com_item_nao_encontrado()
    {
        var carrinho = Dados.CarrinhoCom((Dados.Produto(id: 1), 1));

        carrinho.AlterarQuantidadeItem(produtoId: 99, quantidade: 2)
            .DeveFalharCom(CarrinhoErros.ItemNaoEncontrado(99));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Alterar_para_quantidade_menor_ou_igual_a_zero_falha_sem_alterar_o_carrinho(int quantidade)
    {
        var produto = Dados.Produto();
        var carrinho = Dados.CarrinhoCom((produto, 2));
        var antes = carrinho.Retrato();

        carrinho.AlterarQuantidadeItem(produto.Id, quantidade).DeveFalharCom(CarrinhoErros.QuantidadeInvalida);

        carrinho.Retrato().ShouldBe(antes);
    }

    [Fact]
    public void Alterar_para_quantidade_acima_do_estoque_falha_e_igual_ao_estoque_e_permitido()
    {
        var produto = Dados.Produto(estoque: 4);
        var carrinho = Dados.CarrinhoCom((produto, 1));

        carrinho.AlterarQuantidadeItem(produto.Id, 5)
            .DeveFalharComCodigo("produto.estoque_insuficiente", ErrorType.BusinessRule);
        carrinho.Itens.ShouldHaveSingleItem().Quantidade.ShouldBe(1);

        carrinho.AlterarQuantidadeItem(produto.Id, 4).DeveTerSucesso();
        carrinho.Itens.ShouldHaveSingleItem().Quantidade.ShouldBe(4);
    }

    [Fact]
    public void Remover_produto_retira_a_linha_inteira()
    {
        var produtoA = Dados.Produto(id: 1);
        var produtoB = Dados.Produto(id: 2);
        var carrinho = Dados.CarrinhoCom((produtoA, 3), (produtoB, 1));

        carrinho.RemoverItem(produtoA.Id).DeveTerSucesso();

        carrinho.Itens.ShouldHaveSingleItem().ProdutoId.ShouldBe(produtoB.Id);
    }

    [Fact]
    public void Remover_produto_que_nao_esta_no_carrinho_falha_com_item_nao_encontrado()
    {
        var carrinho = Dados.CarrinhoCom((Dados.Produto(id: 1), 1));
        var antes = carrinho.Retrato();

        var resultado = carrinho.RemoverItem(produtoId: 42);

        resultado.DeveFalharCom(CarrinhoErros.ItemNaoEncontrado(42));
        resultado.Error!.Type.ShouldBe(ErrorType.NotFound);
        carrinho.Retrato().ShouldBe(antes);
    }
}

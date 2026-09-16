using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Domain.Tests.Produtos;

public sealed class ProdutoTests
{
    [Fact]
    public void Cria_produto_com_os_valores_informados()
    {
        var produto = new Produto(7, "Escudo de Carvalho Reforçado", 89.00m, 3);

        produto.Id.ShouldBe(7);
        produto.DescricaoProduto.ShouldBe("Escudo de Carvalho Reforçado");
        produto.PrecoLiquido.ShouldBe(89.00m);
        produto.QuantidadeEstoque.ShouldBe(3);
    }

    [Theory]
    [InlineData(0, "Produto", 10, 1)]
    [InlineData(-1, "Produto", 10, 1)]
    [InlineData(1, "Produto", -0.01, 1)]
    [InlineData(1, "Produto", 10.555, 1)]
    [InlineData(1, "Produto", 10, -1)]
    public void Rejeita_dados_invalidos(int id, string descricao, decimal preco, int estoque) =>
        Should.Throw<ArgumentException>(() => new Produto(id, descricao, preco, estoque));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejeita_descricao_vazia(string descricao) =>
        Should.Throw<ArgumentException>(() => new Produto(1, descricao, 10m, 1));

    [Fact]
    public void Rejeita_descricao_maior_que_o_limite() =>
        Should.Throw<ArgumentException>(() => new Produto(1, new string('x', Produto.DescricaoTamanhoMaximo + 1), 10m, 1));

    [Theory]
    [InlineData(5, 5, true)]
    [InlineData(5, 4, true)]
    [InlineData(5, 6, false)]
    [InlineData(0, 1, false)]
    public void Informa_se_o_estoque_comporta_a_quantidade(int estoque, long quantidade, bool esperado) =>
        new Produto(1, "Produto", 10m, estoque).PossuiDisponivelPara(quantidade).ShouldBe(esperado);
}

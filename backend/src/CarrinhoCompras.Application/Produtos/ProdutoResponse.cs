using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Application.Produtos;

/// <summary>Produto do catálogo.</summary>
/// <param name="Id">Identificador do produto.</param>
/// <param name="DescricaoProduto">Descrição do produto.</param>
/// <param name="PrecoLiquido">Preço líquido unitário.</param>
/// <param name="QuantidadeEstoque">Quantidade disponível em estoque.</param>
public sealed record ProdutoResponse(int Id, string DescricaoProduto, decimal PrecoLiquido, int QuantidadeEstoque)
{
    public static ProdutoResponse De(Produto produto) =>
        new(produto.Id, produto.DescricaoProduto, produto.PrecoLiquido, produto.QuantidadeEstoque);
}

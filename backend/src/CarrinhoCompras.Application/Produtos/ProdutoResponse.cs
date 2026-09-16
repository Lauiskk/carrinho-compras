using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Application.Produtos;

/// <summary>Produto do catálogo.</summary>
/// <param name="Id">Identificador do produto.</param>
/// <param name="DescricaoProduto">Descrição do produto.</param>
/// <param name="PrecoLiquido">Preço líquido unitário.</param>
/// <param name="QuantidadeEstoque">Unidades físicas em estoque.</param>
/// <param name="QuantidadeReservada">Unidades presas em sacolas abertas.</param>
/// <param name="QuantidadeDisponivel">Quantidade disponível em estoque: estoque menos o que está reservado.</param>
public sealed record ProdutoResponse(
    int Id,
    string DescricaoProduto,
    decimal PrecoLiquido,
    int QuantidadeEstoque,
    int QuantidadeReservada,
    int QuantidadeDisponivel)
{
    public static ProdutoResponse De(Produto produto) => new(
        produto.Id,
        produto.DescricaoProduto,
        produto.PrecoLiquido,
        produto.QuantidadeEstoque,
        produto.QuantidadeReservada,
        produto.QuantidadeDisponivel);
}

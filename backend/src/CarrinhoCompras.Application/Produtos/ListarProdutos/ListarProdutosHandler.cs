using CarrinhoCompras.Application.Common;

namespace CarrinhoCompras.Application.Produtos.ListarProdutos;

public sealed class ListarProdutosHandler(IProdutoRepository produtos)
{
    public async Task<IReadOnlyList<ProdutoResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var catalogo = await produtos.ListarAsync(cancellationToken);
        return [.. catalogo.Select(ProdutoResponse.De)];
    }
}

using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Application.Produtos.ObterProduto;

public sealed class ObterProdutoHandler(IProdutoRepository produtos)
{
    public async Task<Result<ProdutoResponse>> HandleAsync(int produtoId, CancellationToken cancellationToken)
    {
        var produto = await produtos.ObterAsync(produtoId, cancellationToken);
        return produto is null
            ? ProdutoErros.NaoEncontrado(produtoId)
            : ProdutoResponse.De(produto);
    }
}

using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Application.Common;

public interface IProdutoRepository
{
    Task<Produto?> ObterAsync(int produtoId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken);
}

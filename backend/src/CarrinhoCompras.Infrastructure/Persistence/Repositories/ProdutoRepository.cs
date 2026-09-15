using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Produtos;
using Microsoft.EntityFrameworkCore;

namespace CarrinhoCompras.Infrastructure.Persistence.Repositories;

internal sealed class ProdutoRepository(CarrinhoComprasDbContext contexto) : IProdutoRepository
{
    // Rastreado: o produto pode ser ligado a um item do carrinho. FindAsync reaproveita a instância
    // se ela já foi carregada junto com o carrinho, sem nova ida ao banco.
    public async Task<Produto?> ObterAsync(int produtoId, CancellationToken cancellationToken) =>
        await contexto.Produtos.FindAsync([produtoId], cancellationToken);

    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken) =>
        await contexto.Produtos
            .AsNoTracking()
            .OrderBy(produto => produto.Id)
            .ToListAsync(cancellationToken);
}

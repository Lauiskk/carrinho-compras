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

    /// <summary>
    /// <c>SELECT ... FOR UPDATE</c> nas linhas dos produtos envolvidos, <b>em ordem de ID</b>: como toda
    /// operação toma os bloqueios na mesma ordem, duas requisições nunca ficam esperando uma pela outra.
    /// O <c>0</c> no lugar do produto adicional não casa com nenhum ID (eles começam em 1).
    /// </summary>
    public Task BloquearParaAlterarEstoqueAsync(Guid carrinhoId, int? produtoAdicional, CancellationToken cancellationToken) =>
        contexto.Database.ExecuteSqlAsync(
            $"""
             SELECT 1 FROM "Produto" produto
             WHERE produto."ID" IN (SELECT item."ProdutoID" FROM "ItemCarrinho" item WHERE item."CarrinhoID" = {carrinhoId})
                OR produto."ID" = {produtoAdicional ?? 0}
             ORDER BY produto."ID"
             FOR UPDATE
             """,
            cancellationToken);

    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken) =>
        await contexto.Produtos
            .AsNoTracking()
            .OrderBy(produto => produto.Id)
            .ToListAsync(cancellationToken);
}

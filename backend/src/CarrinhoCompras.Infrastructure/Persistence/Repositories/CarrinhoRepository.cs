using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using Microsoft.EntityFrameworkCore;

namespace CarrinhoCompras.Infrastructure.Persistence.Repositories;

internal sealed class CarrinhoRepository(CarrinhoComprasDbContext contexto) : ICarrinhoRepository
{
    public Task<Carrinho?> ObterAsync(Guid carrinhoId, CancellationToken cancellationToken) =>
        contexto.Carrinhos
            .Include(carrinho => carrinho.Itens)
                .ThenInclude(item => item.Produto)
            .Include(carrinho => carrinho.Cupom)
            .FirstOrDefaultAsync(carrinho => carrinho.Id == carrinhoId, cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListarIdsComReservaVencidaAsync(
        DateTimeOffset agora, int limite, CancellationToken cancellationToken) =>
        await contexto.Carrinhos
            .AsNoTracking()
            .Where(carrinho => carrinho.Status == StatusCarrinho.Aberto
                && carrinho.ExpiraEm != null
                && carrinho.ExpiraEm <= agora)
            .OrderBy(carrinho => carrinho.ExpiraEm)
            .Take(limite)
            .Select(carrinho => carrinho.Id)
            .ToListAsync(cancellationToken);

    public void Adicionar(Carrinho carrinho) => contexto.Carrinhos.Add(carrinho);
}

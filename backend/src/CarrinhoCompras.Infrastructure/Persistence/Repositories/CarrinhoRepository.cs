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

    public void Adicionar(Carrinho carrinho) => contexto.Carrinhos.Add(carrinho);
}

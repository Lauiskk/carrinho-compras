using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Cupons;
using Microsoft.EntityFrameworkCore;

namespace CarrinhoCompras.Infrastructure.Persistence.Repositories;

internal sealed class CupomRepository(CarrinhoComprasDbContext contexto) : ICupomRepository
{
    public Task<Cupom?> ObterPorCodigoAsync(string codigoCupom, CancellationToken cancellationToken) =>
        contexto.Cupons.FirstOrDefaultAsync(cupom => cupom.CodigoCupom == codigoCupom, cancellationToken);
}

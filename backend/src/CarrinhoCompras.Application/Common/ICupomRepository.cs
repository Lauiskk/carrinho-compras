using CarrinhoCompras.Domain.Cupons;

namespace CarrinhoCompras.Application.Common;

public interface ICupomRepository
{
    /// <summary>Busca pelo código na forma canônica (ver <see cref="Cupom.NormalizarCodigo"/>).</summary>
    Task<Cupom?> ObterPorCodigoAsync(string codigoCupom, CancellationToken cancellationToken);
}

using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.ObterCarrinho;

/// <summary>
/// Leitura pura: uma consulta não expira nada nem mexe em estoque. Quem devolve as unidades de uma sacola
/// vencida é o serviço em segundo plano (a cada 30s) ou a própria tentativa de alterá-la. O campo
/// <c>expiraEm</c> vai na resposta para o cliente saber quanto tempo ainda resta.
/// </summary>
public sealed class ObterCarrinhoHandler(ICarrinhoRepository carrinhos)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(Guid carrinhoId, CancellationToken cancellationToken)
    {
        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        return carrinho is null
            ? CarrinhoErros.NaoEncontrado(carrinhoId)
            : CarrinhoResponse.De(carrinho);
    }
}

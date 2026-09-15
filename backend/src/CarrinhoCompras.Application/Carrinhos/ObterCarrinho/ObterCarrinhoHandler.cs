using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.ObterCarrinho;

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

using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.RemoverItem;

public sealed class RemoverItemHandler(ICarrinhoRepository carrinhos, IUnitOfWork unitOfWork)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(Guid carrinhoId, int produtoId, CancellationToken cancellationToken)
    {
        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        if (carrinho is null)
        {
            return CarrinhoErros.NaoEncontrado(carrinhoId);
        }

        var resultado = carrinho.RemoverItem(produtoId);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

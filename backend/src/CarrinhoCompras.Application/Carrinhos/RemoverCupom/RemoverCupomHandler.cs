using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.RemoverCupom;

public sealed class RemoverCupomHandler(ICarrinhoRepository carrinhos, IUnitOfWork unitOfWork)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(Guid carrinhoId, CancellationToken cancellationToken)
    {
        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        if (carrinho is null)
        {
            return CarrinhoErros.NaoEncontrado(carrinhoId);
        }

        var resultado = carrinho.RemoverCupom();
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

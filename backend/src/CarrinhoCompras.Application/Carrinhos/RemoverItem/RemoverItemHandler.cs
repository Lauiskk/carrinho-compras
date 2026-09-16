using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.RemoverItem;

public sealed class RemoverItemHandler(
    ICarrinhoRepository carrinhos, IProdutoRepository produtos, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(Guid carrinhoId, int produtoId, CancellationToken cancellationToken)
    {
        await unitOfWork.IniciarTransacaoAsync(cancellationToken);
        await produtos.BloquearParaAlterarEstoqueAsync(carrinhoId, produtoId, cancellationToken);

        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        if (carrinho is null)
        {
            return CarrinhoErros.NaoEncontrado(carrinhoId);
        }

        var resultado = carrinho.RemoverItem(produtoId, timeProvider.AgoraUtc());
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

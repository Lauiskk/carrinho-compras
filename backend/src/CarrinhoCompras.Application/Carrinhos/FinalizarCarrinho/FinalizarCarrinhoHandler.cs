using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.FinalizarCarrinho;

public sealed class FinalizarCarrinhoHandler(
    ICarrinhoRepository carrinhos, IProdutoRepository produtos, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(Guid carrinhoId, CancellationToken cancellationToken)
    {
        await unitOfWork.IniciarTransacaoAsync(cancellationToken);
        await produtos.BloquearParaAlterarEstoqueAsync(carrinhoId, null, cancellationToken);

        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        if (carrinho is null)
        {
            return CarrinhoErros.NaoEncontrado(carrinhoId);
        }

        var agora = timeProvider.AgoraUtc();
        var podeAlterar = await carrinho.GarantirAlteravelAsync(agora, unitOfWork, cancellationToken);
        if (podeAlterar.IsFailure)
        {
            return podeAlterar.Error;
        }

        var resultado = carrinho.Finalizar(agora);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

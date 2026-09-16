using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.RemoverCupom;

public sealed class RemoverCupomHandler(
    ICarrinhoRepository carrinhos,
    IProdutoRepository produtos,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    PoliticaDeReserva politica)
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

        var resultado = carrinho.RemoverCupom(agora, politica.Janela);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

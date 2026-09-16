using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Cupons;

namespace CarrinhoCompras.Application.Carrinhos.AplicarCupom;

public sealed class AplicarCupomHandler(
    ICarrinhoRepository carrinhos,
    ICupomRepository cupons,
    IProdutoRepository produtos,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    PoliticaDeReserva politica)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(
        Guid carrinhoId, AplicarCupomRequest request, CancellationToken cancellationToken)
    {
        var agora = timeProvider.AgoraUtc();
        await unitOfWork.IniciarTransacaoAsync(cancellationToken);
        await produtos.BloquearParaAlterarEstoqueAsync(carrinhoId, null, cancellationToken);

        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        if (carrinho is null)
        {
            return CarrinhoErros.NaoEncontrado(carrinhoId);
        }

        var podeAlterar = await carrinho.GarantirAlteravelAsync(agora, unitOfWork, cancellationToken);
        if (podeAlterar.IsFailure)
        {
            return podeAlterar.Error;
        }

        var codigo = Cupom.NormalizarCodigo(request.CodigoCupom);
        var cupom = await cupons.ObterPorCodigoAsync(codigo, cancellationToken);
        if (cupom is null)
        {
            return CupomErros.Invalido(codigo);
        }

        var resultado = carrinho.AplicarCupom(cupom, agora, politica.Janela);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

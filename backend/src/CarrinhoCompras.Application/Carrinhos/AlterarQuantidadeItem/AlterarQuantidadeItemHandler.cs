using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Application.Carrinhos.AlterarQuantidadeItem;

public sealed class AlterarQuantidadeItemHandler(
    ICarrinhoRepository carrinhos,
    IProdutoRepository produtos,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    PoliticaDeReserva politica)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(
        Guid carrinhoId, int produtoId, AlterarQuantidadeItemRequest request, CancellationToken cancellationToken)
    {
        await unitOfWork.IniciarTransacaoAsync(cancellationToken);
        await produtos.BloquearParaAlterarEstoqueAsync(carrinhoId, produtoId, cancellationToken);

        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        if (carrinho is null)
        {
            return CarrinhoErros.NaoEncontrado(carrinhoId);
        }

        var resultado = carrinho.AlterarQuantidadeItem(produtoId, request.Quantidade, timeProvider.AgoraUtc(), politica.Janela);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

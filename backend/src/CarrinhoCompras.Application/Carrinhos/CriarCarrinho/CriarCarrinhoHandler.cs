using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;

namespace CarrinhoCompras.Application.Carrinhos.CriarCarrinho;

public sealed class CriarCarrinhoHandler(ICarrinhoRepository carrinhos, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    public async Task<CarrinhoResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var carrinho = Carrinho.Criar(timeProvider.AgoraUtc());

        carrinhos.Adicionar(carrinho);
        await unitOfWork.CommitAsync(cancellationToken);

        return CarrinhoResponse.De(carrinho);
    }
}

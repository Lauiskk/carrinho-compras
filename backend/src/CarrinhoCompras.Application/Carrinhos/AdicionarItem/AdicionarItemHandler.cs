using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Application.Carrinhos.AdicionarItem;

public sealed class AdicionarItemHandler(
    ICarrinhoRepository carrinhos, IProdutoRepository produtos, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    public async Task<Result<CarrinhoResponse>> HandleAsync(
        Guid carrinhoId, AdicionarItemRequest request, CancellationToken cancellationToken)
    {
        var agora = timeProvider.AgoraUtc();
        await unitOfWork.IniciarTransacaoAsync(cancellationToken);
        await produtos.BloquearParaAlterarEstoqueAsync(carrinhoId, request.ProdutoId, cancellationToken);

        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);
        if (carrinho is null)
        {
            return CarrinhoErros.NaoEncontrado(carrinhoId);
        }

        var podeAlterar = carrinho.VerificarSePodeSerAlterado(agora);
        if (podeAlterar.IsFailure)
        {
            return podeAlterar.Error;
        }

        var produto = await produtos.ObterAsync(request.ProdutoId, cancellationToken);
        if (produto is null)
        {
            return ProdutoErros.NaoEncontrado(request.ProdutoId);
        }

        var resultado = carrinho.AdicionarItem(produto, request.Quantidade, agora);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return CarrinhoResponse.De(carrinho);
    }
}

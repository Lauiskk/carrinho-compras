using CarrinhoCompras.Domain.Carrinhos;

namespace CarrinhoCompras.Application.Common;

public interface ICarrinhoRepository
{
    /// <summary>
    /// Carrega o carrinho completo (itens com seus produtos e o cupom ativo), pronto para ser alterado.
    /// </summary>
    Task<Carrinho?> ObterAsync(Guid carrinhoId, CancellationToken cancellationToken);

    void Adicionar(Carrinho carrinho);
}

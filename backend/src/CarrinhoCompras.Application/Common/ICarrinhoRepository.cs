using CarrinhoCompras.Domain.Carrinhos;

namespace CarrinhoCompras.Application.Common;

public interface ICarrinhoRepository
{
    /// <summary>
    /// Carrega o carrinho completo (itens com seus produtos e o cupom ativo), pronto para ser alterado.
    /// </summary>
    Task<Carrinho?> ObterAsync(Guid carrinhoId, CancellationToken cancellationToken);

    /// <summary>
    /// Ids de carrinhos abertos cujo prazo de reserva já venceu, dos mais antigos para os mais novos.
    /// Só os ids: cada um é carregado depois, na sua própria transação, com os produtos bloqueados.
    /// </summary>
    Task<IReadOnlyList<Guid>> ListarIdsComReservaVencidaAsync(
        DateTimeOffset agora, int limite, CancellationToken cancellationToken);

    void Adicionar(Carrinho carrinho);
}

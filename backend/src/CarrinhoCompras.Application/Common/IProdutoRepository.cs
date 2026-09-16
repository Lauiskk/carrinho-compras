using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Application.Common;

public interface IProdutoRepository
{
    Task<Produto?> ObterAsync(int produtoId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reserva o direito de mexer no estoque dos produtos que esta operação pode tocar: os que já estão no
    /// carrinho (uma expiração devolve todos) e, se houver, o que está entrando agora.
    /// <para>
    /// Enquanto a transação durar, outra requisição que queira os mesmos produtos <b>espera</b> em vez de
    /// falhar — numa loja, o item concorrido é justamente o que mais teria conflito.
    /// </para>
    /// </summary>
    /// <remarks>Deve ser chamado depois de <see cref="IUnitOfWork.IniciarTransacaoAsync"/> e antes de carregar o carrinho.</remarks>
    Task BloquearParaAlterarEstoqueAsync(Guid carrinhoId, int? produtoAdicional, CancellationToken cancellationToken);
}

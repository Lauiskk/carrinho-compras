namespace CarrinhoCompras.Application.Common;

/// <summary>Grava, de forma atômica, todas as alterações feitas durante o caso de uso.</summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Abre uma transação, se ainda não houver uma. Necessária quando a operação mexe em estoque: os
    /// bloqueios de linha tomados por <see cref="IProdutoRepository.BloquearParaAlterarEstoqueAsync"/> só
    /// valem até o fim da transação.
    /// </summary>
    Task IniciarTransacaoAsync(CancellationToken cancellationToken);

    /// <exception cref="ConflitoConcorrenciaException">Outra requisição alterou os mesmos dados antes da gravação.</exception>
    Task CommitAsync(CancellationToken cancellationToken);
}

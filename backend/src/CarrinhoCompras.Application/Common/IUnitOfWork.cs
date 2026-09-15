namespace CarrinhoCompras.Application.Common;

/// <summary>Grava, de forma atômica, todas as alterações feitas durante o caso de uso.</summary>
public interface IUnitOfWork
{
    /// <exception cref="ConflitoConcorrenciaException">Outra requisição alterou os mesmos dados antes da gravação.</exception>
    Task CommitAsync(CancellationToken cancellationToken);
}

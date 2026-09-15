namespace CarrinhoCompras.Application.Common;

/// <summary>
/// Outra requisição alterou o mesmo carrinho entre a leitura e a gravação (concorrência otimista).
/// A persistência traduz para esta exceção, para que as camadas de cima não dependam do ORM.
/// </summary>
public sealed class ConflitoConcorrenciaException : Exception
{
    public ConflitoConcorrenciaException()
        : base("Os dados foram alterados por outra operação.")
    {
    }

    public ConflitoConcorrenciaException(string message)
        : base(message)
    {
    }

    public ConflitoConcorrenciaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

namespace CarrinhoCompras.Domain.Common;

/// <summary>
/// Categoria de uma falha esperada. O domínio não conhece HTTP: a API traduz cada tipo para o status adequado.
/// </summary>
public enum ErrorType
{
    /// <summary>Entrada inválida (ex.: quantidade menor ou igual a zero).</summary>
    Validation = 1,

    /// <summary>Recurso inexistente (ex.: carrinho, produto ou item não encontrado).</summary>
    NotFound = 2,

    /// <summary>O estado atual impede a operação (ex.: carrinho finalizado).</summary>
    Conflict = 3,

    /// <summary>Regra de negócio violada (ex.: estoque insuficiente, cupom inválido).</summary>
    BusinessRule = 4,
}

/// <summary>
/// Falha esperada de negócio: um código estável (para quem integra) e uma mensagem clara (para quem usa).
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error BusinessRule(string code, string message) => new(code, message, ErrorType.BusinessRule);
}

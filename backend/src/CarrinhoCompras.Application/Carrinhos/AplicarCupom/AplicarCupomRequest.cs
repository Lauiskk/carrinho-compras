namespace CarrinhoCompras.Application.Carrinhos.AplicarCupom;

/// <summary>Cupom a aplicar no carrinho.</summary>
public sealed record AplicarCupomRequest
{
    /// <summary>Código do cupom (espaços nas pontas e maiúsculas/minúsculas são ignorados).</summary>
    /// <example>10OFF</example>
    public string CodigoCupom { get; init; } = string.Empty;
}

namespace CarrinhoCompras.Application.Carrinhos.AlterarQuantidadeItem;

/// <summary>Nova quantidade de um item que já está no carrinho.</summary>
public sealed record AlterarQuantidadeItemRequest
{
    /// <summary>Quantidade exata desejada (substitui a atual; pode aumentar ou diminuir).</summary>
    /// <example>5</example>
    public int Quantidade { get; init; }
}

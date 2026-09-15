using System.ComponentModel;

namespace CarrinhoCompras.Application.Carrinhos.AdicionarItem;

/// <summary>Produto e quantidade a adicionar ao carrinho.</summary>
public sealed record AdicionarItemRequest
{
    public const int QuantidadePadrao = 1;

    /// <summary>Identificador do produto no catálogo.</summary>
    /// <example>1</example>
    public int ProdutoId { get; init; }

    /// <summary>
    /// Unidades a adicionar (padrão 1). Se o produto já estiver no carrinho, a quantidade é somada à existente.
    /// </summary>
    /// <example>2</example>
    [DefaultValue(QuantidadePadrao)]
    public int Quantidade { get; init; } = QuantidadePadrao;
}

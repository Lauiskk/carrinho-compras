namespace CarrinhoCompras.Domain.Carrinhos;

public enum StatusCarrinho
{
    /// <summary>O carrinho aceita alterações.</summary>
    Aberto = 1,

    /// <summary>Checkout realizado: o carrinho não pode mais ser alterado.</summary>
    Finalizado = 2,
}

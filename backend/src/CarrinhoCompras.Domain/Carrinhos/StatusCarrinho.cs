namespace CarrinhoCompras.Domain.Carrinhos;

public enum StatusCarrinho
{
    /// <summary>O carrinho aceita alterações e segura as unidades dos seus itens.</summary>
    Aberto = 1,

    /// <summary>Checkout realizado: o carrinho não pode mais ser alterado.</summary>
    Finalizado = 2,

    /// <summary>A reserva venceu: as unidades voltaram para a loja e o carrinho não aceita mais alterações.</summary>
    Expirado = 3,
}

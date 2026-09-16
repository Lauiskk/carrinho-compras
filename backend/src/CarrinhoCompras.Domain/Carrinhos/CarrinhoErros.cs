using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Domain.Carrinhos;

public static class CarrinhoErros
{
    public static readonly Error Finalizado = Error.Conflict(
        "carrinho.finalizado",
        "Este carrinho já foi finalizado e não pode mais ser alterado. Crie um novo carrinho para continuar comprando.");

    public static readonly Error Expirado = Error.Conflict(
        "carrinho.expirado",
        "Sua sacola expirou e as mercadorias voltaram para a loja. Comece uma nova compra.");

    public static readonly Error QuantidadeInvalida = Error.Validation(
        "carrinho.quantidade_invalida",
        "A quantidade deve ser maior que zero.");

    public static readonly Error Vazio = Error.BusinessRule(
        "carrinho.vazio",
        "Não é possível finalizar um carrinho sem itens.");

    public static Error NaoEncontrado(Guid carrinhoId) => Error.NotFound(
        "carrinho.nao_encontrado",
        $"Carrinho '{carrinhoId}' não encontrado.");

    public static Error ItemNaoEncontrado(int produtoId) => Error.NotFound(
        "carrinho.item_nao_encontrado",
        $"O produto {produtoId} não está no carrinho.");
}

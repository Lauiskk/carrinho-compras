using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Domain.Produtos;

public static class ProdutoErros
{
    public static Error NaoEncontrado(int produtoId) => Error.NotFound(
        "produto.nao_encontrado",
        $"Produto {produtoId} não encontrado no catálogo.");

    /// <summary>
    /// Falta disponibilidade para as unidades <b>novas</b> que o pedido quer prender — o que já está nesta
    /// sacola segue reservado. O número informado é o disponível (estoque menos o reservado em todas as sacolas).
    /// </summary>
    public static Error EstoqueInsuficiente(Produto produto, long quantidadePedida) => Error.BusinessRule(
        "produto.estoque_insuficiente",
        $"Estoque insuficiente para '{produto.DescricaoProduto}': você pediu mais {quantidadePedida} " +
        $"unidade(s), mas há apenas {produto.QuantidadeDisponivel} disponível(is) no momento.");
}

using CarrinhoCompras.Domain.Common;

namespace CarrinhoCompras.Domain.Produtos;

public static class ProdutoErros
{
    public static Error NaoEncontrado(int produtoId) => Error.NotFound(
        "produto.nao_encontrado",
        $"Produto {produtoId} não encontrado no catálogo.");

    public static Error EstoqueInsuficiente(Produto produto, long quantidadeNoCarrinho) => Error.BusinessRule(
        "produto.estoque_insuficiente",
        $"Estoque insuficiente para '{produto.DescricaoProduto}': o carrinho ficaria com {quantidadeNoCarrinho} " +
        $"unidade(s), mas há apenas {produto.QuantidadeEstoque} em estoque.");
}

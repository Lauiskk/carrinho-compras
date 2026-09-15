using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Domain.Carrinhos;

/// <summary>
/// Linha do carrinho: um produto, sua quantidade e o preço do item (preço unitário × quantidade).
/// Só o <see cref="Carrinho"/> cria e altera itens, garantindo que os totais sejam sempre recalculados.
/// </summary>
public sealed class ItemCarrinho
{
    internal ItemCarrinho(Guid carrinhoId, Produto produto, int quantidade)
    {
        CarrinhoId = carrinhoId;
        ProdutoId = produto.Id;
        Produto = produto;
        DefinirQuantidade(quantidade);
    }

    // Construtor de materialização (usado ao carregar do banco); a navegação Produto é preenchida em seguida.
    private ItemCarrinho(Guid carrinhoId, int produtoId, int quantidade, decimal precoUnitario, decimal precoItem)
    {
        CarrinhoId = carrinhoId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        PrecoItem = precoItem;
    }

    public Guid CarrinhoId { get; private set; }

    public int ProdutoId { get; private set; }

    public Produto Produto { get; private set; } = null!;

    public int Quantidade { get; private set; }

    /// <summary>Preço líquido unitário do produto no momento em que o item foi adicionado ou alterado.</summary>
    public decimal PrecoUnitario { get; private set; }

    /// <summary>Preço do item: preço unitário × quantidade.</summary>
    public decimal PrecoItem { get; private set; }

    internal void DefinirQuantidade(int quantidade)
    {
        Quantidade = quantidade;
        PrecoUnitario = Produto.PrecoLiquido;
        PrecoItem = PrecoUnitario * quantidade;
    }
}

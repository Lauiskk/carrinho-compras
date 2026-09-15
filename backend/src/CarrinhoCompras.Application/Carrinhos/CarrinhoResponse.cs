using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Cupons;

namespace CarrinhoCompras.Application.Carrinhos;

/// <summary>Carrinho com itens, cupom ativo e valores já calculados.</summary>
/// <param name="Id">Identificador do carrinho.</param>
/// <param name="Status">Aberto (aceita alterações) ou Finalizado (checkout realizado).</param>
/// <param name="Itens">Itens do carrinho.</param>
/// <param name="Cupom">Cupom ativo, se houver.</param>
/// <param name="Subtotal">Soma do preço de todos os itens.</param>
/// <param name="Desconto">Desconto do cupom ativo sobre o subtotal.</param>
/// <param name="Total">Subtotal menos desconto.</param>
/// <param name="CriadoEm">Data de criação (UTC).</param>
/// <param name="FinalizadoEm">Data do checkout (UTC), se finalizado.</param>
public sealed record CarrinhoResponse(
    Guid Id,
    StatusCarrinho Status,
    IReadOnlyList<ItemCarrinhoResponse> Itens,
    CupomAplicadoResponse? Cupom,
    decimal Subtotal,
    decimal Desconto,
    decimal Total,
    DateTimeOffset CriadoEm,
    DateTimeOffset? FinalizadoEm)
{
    public static CarrinhoResponse De(Carrinho carrinho) => new(
        carrinho.Id,
        carrinho.Status,
        [.. carrinho.Itens.OrderBy(item => item.ProdutoId).Select(ItemCarrinhoResponse.De)],
        carrinho.Cupom is null ? null : CupomAplicadoResponse.De(carrinho.Cupom),
        carrinho.Subtotal,
        carrinho.Desconto,
        carrinho.Total,
        carrinho.CriadoEm,
        carrinho.FinalizadoEm);
}

/// <summary>Item do carrinho.</summary>
/// <param name="ProdutoId">Identificador do produto.</param>
/// <param name="DescricaoProduto">Descrição do produto.</param>
/// <param name="PrecoLiquidoUnitario">Preço líquido unitário do produto.</param>
/// <param name="QuantidadeEstoque">Quantidade disponível em estoque do produto.</param>
/// <param name="Quantidade">Quantidade do produto no carrinho.</param>
/// <param name="PrecoItem">Preço do item: preço unitário × quantidade.</param>
public sealed record ItemCarrinhoResponse(
    int ProdutoId,
    string DescricaoProduto,
    decimal PrecoLiquidoUnitario,
    int QuantidadeEstoque,
    int Quantidade,
    decimal PrecoItem)
{
    public static ItemCarrinhoResponse De(ItemCarrinho item) => new(
        item.ProdutoId,
        item.Produto.DescricaoProduto,
        item.PrecoUnitario,
        item.Produto.QuantidadeEstoque,
        item.Quantidade,
        item.PrecoItem);
}

/// <summary>Cupom ativo no carrinho.</summary>
/// <param name="CodigoCupom">Código do cupom.</param>
/// <param name="PercentualDesconto">Percentual de desconto sobre o subtotal.</param>
public sealed record CupomAplicadoResponse(string CodigoCupom, decimal PercentualDesconto)
{
    public static CupomAplicadoResponse De(Cupom cupom) => new(cupom.CodigoCupom, cupom.PercentualDesconto);
}

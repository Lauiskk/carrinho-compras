using CarrinhoCompras.Application.Produtos;
using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;
using CarrinhoCompras.Infrastructure.Persistence.Seed;

namespace CarrinhoCompras.Api.IntegrationTests.Suporte;

/// <summary>
/// Dados de referência lidos dos mesmos arquivos JSON que semeiam o banco. Servem para o que não muda —
/// id, descrição e preço.
/// <para>
/// <b>Quantidades não vêm daqui.</b> Com reserva de estoque, o número disponível é dado vivo: cai quando
/// outra sacola pega unidades e o estoque físico cai em cada checkout. Quem precisa de um produto com certa
/// folga usa <see cref="ClienteApi.ProdutoComDisponivelAsync"/>, que pergunta à API o estado do momento.
/// </para>
/// </summary>
internal static class Catalogo
{
    public static IReadOnlyList<Produto> Produtos { get; } = CatalogoSeed.Produtos();

    public static IReadOnlyList<Cupom> Cupons { get; } = CatalogoSeed.Cupons();

    public static int IdInexistente => Produtos.Max(produto => produto.Id) + 1000;

    /// <summary>Identidade do catálogo: o que o seed gravou e nenhuma compra altera.</summary>
    public static (int Id, string Descricao, decimal Preco) Identidade(this ProdutoResponse produto) =>
        (produto.Id, produto.DescricaoProduto, produto.PrecoLiquido);

    public static (int Id, string Descricao, decimal Preco) Identidade(this Produto produto) =>
        (produto.Id, produto.DescricaoProduto, produto.PrecoLiquido);

    public static decimal Desconto(decimal subtotal, Cupom cupom) =>
        decimal.Round(subtotal * cupom.PercentualDesconto / 100m, 2, MidpointRounding.AwayFromZero);
}

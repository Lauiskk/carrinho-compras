using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;
using CarrinhoCompras.Infrastructure.Persistence.Seed;

namespace CarrinhoCompras.Api.IntegrationTests.Suporte;

/// <summary>
/// Dados de referência lidos dos mesmos arquivos JSON que semeiam o banco. Os testes escolhem produtos pelas
/// características de que precisam (ex.: estoque mínimo), então continuam válidos se o catálogo for trocado.
/// </summary>
internal static class Catalogo
{
    public static IReadOnlyList<Produto> Produtos { get; } = CatalogoSeed.Produtos();

    public static IReadOnlyList<Cupom> Cupons { get; } = CatalogoSeed.Cupons();

    public static Produto ComEstoqueDePeloMenos(int quantidade, Produto? diferenteDe = null) =>
        Produtos.FirstOrDefault(produto => produto.QuantidadeEstoque >= quantidade && produto.Id != diferenteDe?.Id)
        ?? throw new InvalidOperationException($"O catálogo não tem produto com estoque de pelo menos {quantidade}.");

    public static Produto ComMenorEstoquePositivo() =>
        Produtos.Where(produto => produto.QuantidadeEstoque > 0).MinBy(produto => produto.QuantidadeEstoque)
        ?? throw new InvalidOperationException("O catálogo não tem produto com estoque.");

    public static int IdInexistente => Produtos.Max(produto => produto.Id) + 1000;

    public static decimal Desconto(decimal subtotal, Cupom cupom) =>
        decimal.Round(subtotal * cupom.PercentualDesconto / 100m, 2, MidpointRounding.AwayFromZero);
}

using System.Text.Json;
using System.Text.Json.Serialization;
using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Infrastructure.Persistence.Seed;

/// <summary>
/// Lê os arquivos <c>produtos.json</c> e <c>cupons.json</c> (embutidos no assembly) e os converte em entidades do domínio.
/// A leitura é estrita: tipos errados ou campos ausentes falham na hora, em vez de gravar dados inválidos.
/// </summary>
public static class CatalogoSeed
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.Strict,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    private static readonly Lazy<IReadOnlyList<ProdutoJson>> ProdutosJson = new(() => Ler<ProdutoJson>("produtos.json"));
    private static readonly Lazy<IReadOnlyList<CupomJson>> CuponsJson = new(() => Ler<CupomJson>("cupons.json"));

    /// <summary>Produtos do catálogo, como novas instâncias (prontas para serem rastreadas por um contexto).</summary>
    public static IReadOnlyList<Produto> Produtos() =>
        [.. ProdutosJson.Value.Select(p => new Produto(p.Id, p.DescricaoProduto, p.PrecoLiquido, p.QuantidadeEstoque))];

    /// <summary>Cupons disponíveis, como novas instâncias.</summary>
    public static IReadOnlyList<Cupom> Cupons() =>
        [.. CuponsJson.Value.Select(c => new Cupom(c.Id, c.CodigoCupom, c.PercentualDesconto))];

    private static IReadOnlyList<T> Ler<T>(string arquivo)
    {
        var recurso = $"{typeof(CatalogoSeed).Namespace}.{arquivo}";
        using var stream = typeof(CatalogoSeed).Assembly.GetManifestResourceStream(recurso)
            ?? throw new InvalidOperationException($"Arquivo de seed '{arquivo}' não encontrado no assembly (recurso '{recurso}').");

        return JsonSerializer.Deserialize<List<T>>(stream, OpcoesJson)
            ?? throw new InvalidOperationException($"O arquivo de seed '{arquivo}' está vazio.");
    }

    private sealed record ProdutoJson(int Id, string DescricaoProduto, int QuantidadeEstoque, decimal PrecoLiquido);

    private sealed record CupomJson(int Id, string CodigoCupom, decimal PercentualDesconto);
}

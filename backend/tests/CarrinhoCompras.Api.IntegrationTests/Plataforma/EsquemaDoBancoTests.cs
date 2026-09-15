using CarrinhoCompras.Api.IntegrationTests.Suporte;
using Npgsql;

namespace CarrinhoCompras.Api.IntegrationTests.Plataforma;

/// <summary>
/// O desafio pede nomes apropriados para o banco (tabela Carrinho; colunas ID, DescricaoProduto, PrecoLiquido,
/// QuantidadeEstoque, CodigoCupom, PercentualDesconto) e os mesmos valores/tipos dos JSON.
/// </summary>
public sealed class EsquemaDoBancoTests(ApiFactory api)
{
    [Theory]
    [InlineData("Produto", "ID", "integer")]
    [InlineData("Produto", "DescricaoProduto", "character varying")]
    [InlineData("Produto", "PrecoLiquido", "numeric(18,2)")]
    [InlineData("Produto", "QuantidadeEstoque", "integer")]
    [InlineData("Cupom", "ID", "integer")]
    [InlineData("Cupom", "CodigoCupom", "character varying")]
    [InlineData("Cupom", "PercentualDesconto", "numeric(5,2)")]
    [InlineData("Carrinho", "ID", "uuid")]
    [InlineData("Carrinho", "Status", "character varying")]
    [InlineData("Carrinho", "CupomID", "integer")]
    [InlineData("Carrinho", "Subtotal", "numeric(18,2)")]
    [InlineData("Carrinho", "Desconto", "numeric(18,2)")]
    [InlineData("Carrinho", "Total", "numeric(18,2)")]
    [InlineData("ItemCarrinho", "CarrinhoID", "uuid")]
    [InlineData("ItemCarrinho", "ProdutoID", "integer")]
    [InlineData("ItemCarrinho", "Quantidade", "integer")]
    [InlineData("ItemCarrinho", "PrecoUnitario", "numeric(18,2)")]
    [InlineData("ItemCarrinho", "PrecoItem", "numeric(18,2)")]
    public async Task Tabelas_e_colunas_tem_os_nomes_e_tipos_esperados(string tabela, string coluna, string tipoEsperado)
    {
        const string sql = """
            SELECT CASE WHEN data_type = 'numeric' THEN format('numeric(%s,%s)', numeric_precision, numeric_scale) ELSE data_type END
            FROM information_schema.columns
            WHERE table_schema = current_schema() AND table_name = @tabela AND column_name = @coluna
            """;

        await using var conexao = new NpgsqlConnection(api.ConnectionString);
        await conexao.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("tabela", tabela);
        comando.Parameters.AddWithValue("coluna", coluna);

        var tipo = await comando.ExecuteScalarAsync(TestContext.Current.CancellationToken) as string;

        tipo.ShouldBe(tipoEsperado, $"Coluna \"{tabela}\".\"{coluna}\"");
    }

    [Fact]
    public async Task Tabela_Cupom_tem_os_mesmos_dados_do_cupons_json()
    {
        await using var conexao = new NpgsqlConnection(api.ConnectionString);
        await conexao.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand(
            """SELECT "ID", "CodigoCupom", "PercentualDesconto" FROM "Cupom" ORDER BY "ID" """, conexao);
        await using var leitor = await comando.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        var noBanco = new List<(int, string, decimal)>();
        while (await leitor.ReadAsync(TestContext.Current.CancellationToken))
        {
            noBanco.Add((leitor.GetInt32(0), leitor.GetString(1), leitor.GetDecimal(2)));
        }

        noBanco.ShouldBe(Suporte.Catalogo.Cupons.OrderBy(c => c.Id).Select(c => (c.Id, c.CodigoCupom, c.PercentualDesconto)));
    }
}

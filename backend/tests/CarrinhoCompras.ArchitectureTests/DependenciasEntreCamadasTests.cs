using System.Reflection;

namespace CarrinhoCompras.ArchitectureTests;

/// <summary>
/// Garante a direção das dependências da Clean Architecture olhando as referências reais de cada assembly compilado:
/// Api → Application → Domain, com a Infrastructure implementando as abstrações da Application.
/// </summary>
public sealed class DependenciasEntreCamadasTests
{
    private const string Domain = "CarrinhoCompras.Domain";
    private const string Application = "CarrinhoCompras.Application";
    private const string Infrastructure = "CarrinhoCompras.Infrastructure";
    private const string Api = "CarrinhoCompras.Api";

    [Fact]
    public void Domain_so_depende_da_biblioteca_padrao_do_dotnet()
    {
        var referencias = ReferenciasDe(Domain);

        referencias.ShouldAllBe(
            nome => nome.StartsWith("System", StringComparison.Ordinal) || nome == "netstandard",
            $"O domínio deve ser livre de infraestrutura, mas referencia: {string.Join(", ", referencias)}");
    }

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Npgsql")]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData(Infrastructure)]
    [InlineData(Api)]
    public void Application_nao_depende_de_infraestrutura_nem_de_http(string proibido) =>
        ReferenciasDe(Application).ShouldNotContain(
            nome => nome.StartsWith(proibido, StringComparison.Ordinal),
            $"A camada Application não deve referenciar '{proibido}'.");

    [Theory]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData(Api)]
    public void Infrastructure_nao_depende_da_camada_http(string proibido) =>
        ReferenciasDe(Infrastructure).ShouldNotContain(
            nome => nome.StartsWith(proibido, StringComparison.Ordinal),
            $"A camada Infrastructure não deve referenciar '{proibido}'.");

    private static string[] ReferenciasDe(string assembly) =>
        [.. Assembly.Load(assembly).GetReferencedAssemblies().Select(referencia => referencia.Name!).Order()];
}

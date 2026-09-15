using CarrinhoCompras.Domain.Cupons;

namespace CarrinhoCompras.Domain.Tests.Cupons;

public sealed class CupomTests
{
    [Fact]
    public void Cria_cupom_com_codigo_na_forma_canonica()
    {
        var cupom = new Cupom(1, "  10off ", 10m);

        cupom.Id.ShouldBe(1);
        cupom.CodigoCupom.ShouldBe("10OFF");
        cupom.PercentualDesconto.ShouldBe(10m);
    }

    [Theory]
    [InlineData("15OFF", "15OFF")]
    [InlineData(" 15off", "15OFF")]
    [InlineData("15Off  ", "15OFF")]
    public void Normaliza_codigo_removendo_espacos_e_ignorando_maiusculas(string informado, string esperado) =>
        Cupom.NormalizarCodigo(informado).ShouldBe(esperado);

    [Theory]
    [InlineData(0, "10OFF", 10)]
    [InlineData(1, "   ", 10)]
    [InlineData(1, "10OFF", 0)]
    [InlineData(1, "10OFF", -5)]
    [InlineData(1, "10OFF", 100.01)]
    [InlineData(1, "10OFF", 12.345)]
    public void Rejeita_dados_invalidos(int id, string codigo, decimal percentual) =>
        Should.Throw<ArgumentException>(() => new Cupom(id, codigo, percentual));

    [Theory]
    [InlineData(10, 0, 0)]
    [InlineData(10, 100, 10)]
    [InlineData(15, 120.99, 18.15)]
    [InlineData(100, 99.99, 99.99)]
    public void Calcula_desconto_sobre_o_subtotal(decimal percentual, decimal subtotal, decimal esperado) =>
        new Cupom(1, "CUPOM", percentual).CalcularDesconto(subtotal).ShouldBe(esperado);

    [Fact]
    public void Rejeita_subtotal_negativo() =>
        Should.Throw<ArgumentOutOfRangeException>(() => new Cupom(1, "CUPOM", 10m).CalcularDesconto(-1m));
}

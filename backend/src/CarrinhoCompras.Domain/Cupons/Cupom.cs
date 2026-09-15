namespace CarrinhoCompras.Domain.Cupons;

/// <summary>
/// Cupom de desconto percentual aplicado sobre o subtotal do carrinho.
/// </summary>
public sealed class Cupom
{
    public const int CodigoTamanhoMaximo = 50;

    public Cupom(int id, string codigoCupom, decimal percentualDesconto)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoCupom);

        var codigoNormalizado = NormalizarCodigo(codigoCupom);
        if (codigoNormalizado.Length > CodigoTamanhoMaximo)
        {
            throw new ArgumentException(
                $"O código do cupom deve ter no máximo {CodigoTamanhoMaximo} caracteres.", nameof(codigoCupom));
        }

        if (percentualDesconto is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percentualDesconto), percentualDesconto, "O percentual de desconto deve ser maior que 0 e no máximo 100.");
        }

        if (decimal.Round(percentualDesconto, 2) != percentualDesconto)
        {
            throw new ArgumentException("O percentual de desconto deve ter no máximo 2 casas decimais.", nameof(percentualDesconto));
        }

        Id = id;
        CodigoCupom = codigoNormalizado;
        PercentualDesconto = percentualDesconto;
    }

    public int Id { get; private set; }

    /// <summary>Código na forma canônica (sem espaços nas pontas e em maiúsculas).</summary>
    public string CodigoCupom { get; private set; }

    public decimal PercentualDesconto { get; private set; }

    /// <summary>
    /// Forma canônica de um código de cupom: "  10off " e "10OFF" representam o mesmo cupom.
    /// </summary>
    public static string NormalizarCodigo(string codigoCupom)
    {
        ArgumentNullException.ThrowIfNull(codigoCupom);
        return codigoCupom.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Desconto sobre o subtotal, arredondado para 2 casas com a regra comercial (meio para longe de zero).
    /// </summary>
    public decimal CalcularDesconto(decimal subtotal)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subtotal);
        return decimal.Round(subtotal * PercentualDesconto / 100m, 2, MidpointRounding.AwayFromZero);
    }
}

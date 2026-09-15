namespace CarrinhoCompras.Domain.Produtos;

/// <summary>
/// Produto do catálogo. O estoque é o limite de unidades que um carrinho pode conter deste produto.
/// </summary>
public sealed class Produto
{
    public const int DescricaoTamanhoMaximo = 200;

    public Produto(int id, string descricaoProduto, decimal precoLiquido, int quantidadeEstoque)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(descricaoProduto);
        if (descricaoProduto.Length > DescricaoTamanhoMaximo)
        {
            throw new ArgumentException(
                $"A descrição do produto deve ter no máximo {DescricaoTamanhoMaximo} caracteres.", nameof(descricaoProduto));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(precoLiquido);
        if (decimal.Round(precoLiquido, 2) != precoLiquido)
        {
            // A coluna é numeric(18,2): mais casas seriam arredondadas silenciosamente pelo banco.
            throw new ArgumentException("O preço líquido deve ter no máximo 2 casas decimais.", nameof(precoLiquido));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(quantidadeEstoque);

        Id = id;
        DescricaoProduto = descricaoProduto;
        PrecoLiquido = precoLiquido;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public int Id { get; private set; }

    public string DescricaoProduto { get; private set; }

    /// <summary>Preço líquido unitário.</summary>
    public decimal PrecoLiquido { get; private set; }

    /// <summary>Quantidade disponível em estoque.</summary>
    public int QuantidadeEstoque { get; private set; }

    /// <summary>
    /// Indica se o estoque comporta a quantidade informada. Recebe <see langword="long"/> para que somas
    /// de quantidades (atual + adicional) nunca estourem o limite de <see langword="int"/>.
    /// </summary>
    public bool PossuiEstoquePara(long quantidade) => quantidade <= QuantidadeEstoque;
}

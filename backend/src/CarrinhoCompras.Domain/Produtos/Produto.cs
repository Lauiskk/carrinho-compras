using System.Diagnostics.CodeAnalysis;

namespace CarrinhoCompras.Domain.Produtos;

/// <summary>
/// Produto do catálogo.
/// <para>
/// O estoque tem dois números: <see cref="QuantidadeEstoque"/> são as unidades físicas da loja e
/// <see cref="QuantidadeReservada"/> são as que já estão presas em sacolas abertas. Quem chega na vitrine
/// enxerga a diferença entre os dois (<see cref="QuantidadeDisponivel"/>).
/// </para>
/// <para>
/// O ciclo é: <see cref="Reservar"/> ao entrar numa sacola, <see cref="LiberarReserva"/> ao sair dela ou ao
/// a sacola expirar, e <see cref="ConfirmarVenda"/> no checkout — aí a reserva vira baixa de verdade.
/// </para>
/// </summary>
public sealed class Produto
{
    public const int DescricaoTamanhoMaximo = 200;

    public Produto(int id, string descricaoProduto, decimal precoLiquido, int quantidadeEstoque)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        Id = id;
        AtualizarDoCatalogo(descricaoProduto, precoLiquido, quantidadeEstoque);
    }

    public int Id { get; private set; }

    public string DescricaoProduto { get; private set; }

    /// <summary>Preço líquido unitário.</summary>
    public decimal PrecoLiquido { get; private set; }

    /// <summary>Unidades físicas na loja. Só diminui quando uma compra é finalizada.</summary>
    public int QuantidadeEstoque { get; private set; }

    /// <summary>Unidades presas em sacolas abertas. Nunca passa de <see cref="QuantidadeEstoque"/>.</summary>
    public int QuantidadeReservada { get; private set; }

    /// <summary>Unidades que ainda podem ser levadas por alguém: estoque menos o que está reservado.</summary>
    public int QuantidadeDisponivel => QuantidadeEstoque - QuantidadeReservada;

    /// <summary>
    /// Aplica o que vem do catálogo (descrição, preço e estoque físico) <b>sem tocar na reserva</b>: ela é
    /// estado das sacolas abertas, não do arquivo. Usado pelo seed, que sincroniza o banco com os JSON.
    /// </summary>
    [MemberNotNull(nameof(DescricaoProduto))]
    public void AtualizarDoCatalogo(string descricaoProduto, decimal precoLiquido, int quantidadeEstoque)
    {
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
        if (quantidadeEstoque < QuantidadeReservada)
        {
            throw new InvalidOperationException(
                $"O catálogo não pode baixar o estoque de '{descricaoProduto}' para {quantidadeEstoque}: " +
                $"há {QuantidadeReservada} unidade(s) reservada(s) em sacolas abertas.");
        }

        DescricaoProduto = descricaoProduto;
        PrecoLiquido = precoLiquido;
        QuantidadeEstoque = quantidadeEstoque;
    }

    /// <summary>
    /// Indica se ainda há disponibilidade para a quantidade informada. Recebe <see langword="long"/> para que
    /// somas de quantidades (atual + adicional) nunca estourem o limite de <see langword="int"/>.
    /// </summary>
    public bool PossuiDisponivelPara(long quantidade) => quantidade <= QuantidadeDisponivel;

    /// <summary>Prende unidades para uma sacola. Só o <c>Carrinho</c> chama, depois de checar a disponibilidade.</summary>
    internal void Reservar(int quantidade)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantidade);
        if (!PossuiDisponivelPara(quantidade))
        {
            throw new InvalidOperationException(
                $"Não há {quantidade} unidade(s) disponível(is) de '{DescricaoProduto}' para reservar.");
        }

        QuantidadeReservada += quantidade;
    }

    /// <summary>Devolve unidades à vitrine (item removido, quantidade reduzida ou sacola expirada).</summary>
    internal void LiberarReserva(int quantidade)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantidade);
        if (quantidade > QuantidadeReservada)
        {
            throw new InvalidOperationException(
                $"Não há {quantidade} unidade(s) reservada(s) de '{DescricaoProduto}' para liberar.");
        }

        QuantidadeReservada -= quantidade;
    }

    /// <summary>Checkout: a reserva vira venda. O estoque físico cai e não volta.</summary>
    internal void ConfirmarVenda(int quantidade)
    {
        LiberarReserva(quantidade);
        QuantidadeEstoque -= quantidade;
    }
}

using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Domain.Carrinhos;

/// <summary>
/// Raiz do agregado de carrinho. Concentra todas as regras: itens, estoque, cupom, cálculos e finalização.
/// Toda alteração verifica primeiro se o carrinho está aberto e termina recalculando subtotal, desconto e total.
/// </summary>
public sealed class Carrinho
{
    private readonly List<ItemCarrinho> _itens = [];

    private Carrinho(Guid id, DateTimeOffset criadoEm)
    {
        Id = id;
        CriadoEm = criadoEm;
        Status = StatusCarrinho.Aberto;
    }

    public Guid Id { get; private set; }

    public StatusCarrinho Status { get; private set; }

    public IReadOnlyCollection<ItemCarrinho> Itens => _itens.AsReadOnly();

    public int? CupomId { get; private set; }

    /// <summary>Cupom ativo; no máximo um por carrinho.</summary>
    public Cupom? Cupom { get; private set; }

    /// <summary>Soma do preço de todos os itens.</summary>
    public decimal Subtotal { get; private set; }

    /// <summary>Desconto do cupom ativo sobre o subtotal (zero sem cupom).</summary>
    public decimal Desconto { get; private set; }

    /// <summary>Subtotal − desconto.</summary>
    public decimal Total { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset? FinalizadoEm { get; private set; }

    public bool EstaFinalizado => Status == StatusCarrinho.Finalizado;

    public static Carrinho Criar(DateTimeOffset agora) => new(Guid.CreateVersion7(agora), agora);

    /// <summary>
    /// Adiciona o produto com a quantidade informada. Se o produto já estiver no carrinho, soma à quantidade existente.
    /// </summary>
    public Result AdicionarItem(Produto produto, int quantidade)
    {
        ArgumentNullException.ThrowIfNull(produto);

        if (EstaFinalizado)
        {
            return CarrinhoErros.Finalizado;
        }

        if (quantidade <= 0)
        {
            return CarrinhoErros.QuantidadeInvalida;
        }

        var item = BuscarItem(produto.Id);
        var quantidadeResultante = (long)(item?.Quantidade ?? 0) + quantidade;
        if (!produto.PossuiEstoquePara(quantidadeResultante))
        {
            return ProdutoErros.EstoqueInsuficiente(produto, quantidadeResultante);
        }

        if (item is null)
        {
            _itens.Add(new ItemCarrinho(Id, produto, quantidade));
        }
        else
        {
            // Seguro: a quantidade resultante cabe no estoque, que é um int.
            item.DefinirQuantidade((int)quantidadeResultante);
        }

        Recalcular();
        return Result.Success();
    }

    /// <summary>
    /// Substitui a quantidade de um item que já está no carrinho (pode aumentar ou diminuir).
    /// </summary>
    public Result AlterarQuantidadeItem(int produtoId, int quantidade)
    {
        if (EstaFinalizado)
        {
            return CarrinhoErros.Finalizado;
        }

        if (quantidade <= 0)
        {
            return CarrinhoErros.QuantidadeInvalida;
        }

        var item = BuscarItem(produtoId);
        if (item is null)
        {
            return CarrinhoErros.ItemNaoEncontrado(produtoId);
        }

        if (!item.Produto.PossuiEstoquePara(quantidade))
        {
            return ProdutoErros.EstoqueInsuficiente(item.Produto, quantidade);
        }

        item.DefinirQuantidade(quantidade);
        Recalcular();
        return Result.Success();
    }

    public Result RemoverItem(int produtoId)
    {
        if (EstaFinalizado)
        {
            return CarrinhoErros.Finalizado;
        }

        var item = BuscarItem(produtoId);
        if (item is null)
        {
            return CarrinhoErros.ItemNaoEncontrado(produtoId);
        }

        _itens.Remove(item);
        Recalcular();
        return Result.Success();
    }

    /// <summary>
    /// Aplica o cupom. Como só existe um cupom ativo por vez, um cupom aplicado antes é substituído.
    /// </summary>
    public Result AplicarCupom(Cupom cupom)
    {
        ArgumentNullException.ThrowIfNull(cupom);

        if (EstaFinalizado)
        {
            return CarrinhoErros.Finalizado;
        }

        Cupom = cupom;
        CupomId = cupom.Id;
        Recalcular();
        return Result.Success();
    }

    /// <summary>Remove o cupom ativo. Sem cupom aplicado, não há o que remover e a operação não falha.</summary>
    public Result RemoverCupom()
    {
        if (EstaFinalizado)
        {
            return CarrinhoErros.Finalizado;
        }

        Cupom = null;
        CupomId = null;
        Recalcular();
        return Result.Success();
    }

    /// <summary>Checkout: congela o carrinho, que deixa de aceitar alterações.</summary>
    public Result Finalizar(DateTimeOffset agora)
    {
        if (EstaFinalizado)
        {
            return CarrinhoErros.Finalizado;
        }

        if (_itens.Count == 0)
        {
            return CarrinhoErros.Vazio;
        }

        Status = StatusCarrinho.Finalizado;
        FinalizadoEm = agora;
        return Result.Success();
    }

    private ItemCarrinho? BuscarItem(int produtoId) => _itens.Find(item => item.ProdutoId == produtoId);

    private void Recalcular()
    {
        Subtotal = _itens.Sum(item => item.PrecoItem);
        Desconto = Cupom?.CalcularDesconto(Subtotal) ?? 0m;
        Total = Subtotal - Desconto;
    }
}

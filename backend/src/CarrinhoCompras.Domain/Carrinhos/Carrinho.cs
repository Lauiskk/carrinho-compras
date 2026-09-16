using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Domain.Carrinhos;

/// <summary>
/// Raiz do agregado de carrinho. Concentra todas as regras: itens, reserva de estoque, cupom, cálculos,
/// expiração e finalização.
/// <para>
/// Enquanto está aberto, o carrinho <b>segura</b> as unidades dos seus itens: elas saem da vitrine assim que
/// entram na sacola e voltam quando o item sai, quando a quantidade diminui ou quando a sacola expira. No
/// checkout a reserva vira venda e o estoque físico cai de vez.
/// </para>
/// <para>
/// Toda alteração começa expirando a sacola se o prazo venceu, verifica se ela ainda é alterável, e termina
/// renovando o prazo e recalculando subtotal, desconto e total.
/// </para>
/// </summary>
public sealed class Carrinho
{
    /// <summary>
    /// Por quanto tempo uma sacola parada continua segurando as unidades dos seus itens. É o padrão de
    /// negócio; cada operação pode receber outra janela (a aplicação lê a sua da configuração), porque
    /// "quanto tempo a loja segura uma peça" é decisão comercial, não constante de compilação.
    /// </summary>
    public static readonly TimeSpan JanelaDeReservaPadrao = TimeSpan.FromMinutes(15);

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

    /// <summary>
    /// Instante em que a reserva vence. Só existe enquanto o carrinho está aberto e tem itens segurando
    /// unidades — uma sacola vazia não prende nada, então não tem prazo.
    /// </summary>
    public DateTimeOffset? ExpiraEm { get; private set; }

    public bool EstaFinalizado => Status == StatusCarrinho.Finalizado;

    public bool EstaExpirado => Status == StatusCarrinho.Expirado;

    public static Carrinho Criar(DateTimeOffset agora) => new(Guid.CreateVersion7(agora), agora);

    /// <summary>
    /// Se o prazo da reserva venceu, devolve as unidades à loja e encerra o carrinho.
    /// Idempotente: num carrinho já expirado, finalizado ou ainda no prazo, não faz nada.
    /// </summary>
    /// <returns><see langword="true"/> se este chamado foi quem expirou o carrinho.</returns>
    public bool ExpirarSeVencido(DateTimeOffset agora)
    {
        if (Status != StatusCarrinho.Aberto || ExpiraEm is null || ExpiraEm > agora)
        {
            return false;
        }

        foreach (var item in _itens)
        {
            item.Produto.LiberarReserva(item.Quantidade);
        }

        Status = StatusCarrinho.Expirado;
        ExpiraEm = null;
        return true;
    }

    /// <summary>
    /// Expira o carrinho se for o caso e diz se ele ainda aceita alterações. Permite responder
    /// "finalizado" ou "expirado" antes de buscar outros dados (produto, cupom), para que a resposta seja
    /// sempre sobre o estado do carrinho, qualquer que seja o pedido.
    /// </summary>
    public Result VerificarSePodeSerAlterado(DateTimeOffset agora)
    {
        ExpirarSeVencido(agora);

        if (EstaFinalizado)
        {
            return CarrinhoErros.Finalizado;
        }

        return EstaExpirado ? CarrinhoErros.Expirado : Result.Success();
    }

    /// <summary>
    /// Adiciona o produto com a quantidade informada, reservando as unidades. Se o produto já estiver no
    /// carrinho, soma à quantidade existente (as unidades que já estavam lá seguem reservadas).
    /// </summary>
    public Result AdicionarItem(Produto produto, int quantidade, DateTimeOffset agora, TimeSpan? janelaDeReserva = null)
    {
        ArgumentNullException.ThrowIfNull(produto);

        var alteravel = VerificarSePodeSerAlterado(agora);
        if (alteravel.IsFailure)
        {
            return alteravel.Error;
        }

        if (quantidade <= 0)
        {
            return CarrinhoErros.QuantidadeInvalida;
        }

        // Só falta prender as unidades novas: as que já estão nesta sacola continuam reservadas por ela.
        if (!produto.PossuiDisponivelPara(quantidade))
        {
            return ProdutoErros.EstoqueInsuficiente(produto, quantidade);
        }

        produto.Reservar(quantidade);

        var item = BuscarItem(produto.Id);
        if (item is null)
        {
            _itens.Add(new ItemCarrinho(Id, produto, quantidade));
        }
        else
        {
            // Seguro: o que esta sacola já reservou mais o disponível nunca passa do estoque, que é um int.
            item.DefinirQuantidade(item.Quantidade + quantidade);
        }

        Concluir(agora, janelaDeReserva);
        return Result.Success();
    }

    /// <summary>
    /// Substitui a quantidade de um item que já está no carrinho (pode aumentar ou diminuir), reservando
    /// ou devolvendo apenas a diferença.
    /// </summary>
    public Result AlterarQuantidadeItem(int produtoId, int quantidade, DateTimeOffset agora, TimeSpan? janelaDeReserva = null)
    {
        var alteravel = VerificarSePodeSerAlterado(agora);
        if (alteravel.IsFailure)
        {
            return alteravel.Error;
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

        var diferenca = quantidade - item.Quantidade;
        if (diferenca > 0)
        {
            if (!item.Produto.PossuiDisponivelPara(diferenca))
            {
                return ProdutoErros.EstoqueInsuficiente(item.Produto, diferenca);
            }

            item.Produto.Reservar(diferenca);
        }
        else if (diferenca < 0)
        {
            item.Produto.LiberarReserva(-diferenca);
        }

        item.DefinirQuantidade(quantidade);
        Concluir(agora, janelaDeReserva);
        return Result.Success();
    }

    public Result RemoverItem(int produtoId, DateTimeOffset agora, TimeSpan? janelaDeReserva = null)
    {
        var alteravel = VerificarSePodeSerAlterado(agora);
        if (alteravel.IsFailure)
        {
            return alteravel.Error;
        }

        var item = BuscarItem(produtoId);
        if (item is null)
        {
            return CarrinhoErros.ItemNaoEncontrado(produtoId);
        }

        item.Produto.LiberarReserva(item.Quantidade);
        _itens.Remove(item);
        Concluir(agora, janelaDeReserva);
        return Result.Success();
    }

    /// <summary>
    /// Aplica o cupom. Como só existe um cupom ativo por vez, um cupom aplicado antes é substituído.
    /// </summary>
    public Result AplicarCupom(Cupom cupom, DateTimeOffset agora, TimeSpan? janelaDeReserva = null)
    {
        ArgumentNullException.ThrowIfNull(cupom);

        var alteravel = VerificarSePodeSerAlterado(agora);
        if (alteravel.IsFailure)
        {
            return alteravel.Error;
        }

        Cupom = cupom;
        CupomId = cupom.Id;
        Concluir(agora, janelaDeReserva);
        return Result.Success();
    }

    /// <summary>Remove o cupom ativo. Sem cupom aplicado, não há o que remover e a operação não falha.</summary>
    public Result RemoverCupom(DateTimeOffset agora, TimeSpan? janelaDeReserva = null)
    {
        var alteravel = VerificarSePodeSerAlterado(agora);
        if (alteravel.IsFailure)
        {
            return alteravel.Error;
        }

        Cupom = null;
        CupomId = null;
        Concluir(agora, janelaDeReserva);
        return Result.Success();
    }

    /// <summary>
    /// Checkout: a reserva de cada item vira venda (o estoque físico cai) e o carrinho congela,
    /// deixando de aceitar alterações e de ter prazo.
    /// </summary>
    public Result Finalizar(DateTimeOffset agora)
    {
        var alteravel = VerificarSePodeSerAlterado(agora);
        if (alteravel.IsFailure)
        {
            return alteravel.Error;
        }

        if (_itens.Count == 0)
        {
            return CarrinhoErros.Vazio;
        }

        foreach (var item in _itens)
        {
            item.Produto.ConfirmarVenda(item.Quantidade);
        }

        Status = StatusCarrinho.Finalizado;
        FinalizadoEm = agora;
        ExpiraEm = null;
        return Result.Success();
    }

    private ItemCarrinho? BuscarItem(int produtoId) => _itens.Find(item => item.ProdutoId == produtoId);

    /// <summary>Fecha uma alteração: renova o prazo da reserva e recalcula os valores.</summary>
    private void Concluir(DateTimeOffset agora, TimeSpan? janelaDeReserva)
    {
        // Sacola vazia não segura nada, então não tem prazo para vencer.
        ExpiraEm = _itens.Count > 0 ? agora + (janelaDeReserva ?? JanelaDeReservaPadrao) : null;

        Subtotal = _itens.Sum(item => item.PrecoItem);
        Desconto = Cupom?.CalcularDesconto(Subtotal) ?? 0m;
        Total = Subtotal - Desconto;
    }
}

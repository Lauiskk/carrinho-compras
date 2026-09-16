using CarrinhoCompras.Application.Common;

namespace CarrinhoCompras.Application.Carrinhos.ExpirarReservas;

/// <summary>
/// Devolve à loja as unidades presas em sacolas cujo prazo venceu, para que um carrinho abandonado não
/// segure estoque para sempre.
/// <para>
/// Uma consulta ao carrinho não expira nada (leitura é leitura) e uma tentativa de alterá-lo expira só
/// aquele carrinho. Este caso de uso existe para as sacolas que ninguém mais vai abrir — sem ele, o estoque
/// abandonado nunca voltaria para a vitrine.
/// </para>
/// <para>
/// Cada carrinho é tratado numa transação própria: um conflito com quem estiver usando a sacola naquele
/// instante não derruba a varredura inteira.
/// </para>
/// </summary>
public sealed class ExpirarReservasVencidasHandler(
    ICarrinhoRepository carrinhos, IProdutoRepository produtos, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    /// <summary>Quantos carrinhos uma passagem trata, para a varredura não virar um trabalho enorme.</summary>
    public const int TamanhoDoLote = 100;

    public Task<IReadOnlyList<Guid>> ListarVencidosAsync(CancellationToken cancellationToken) =>
        carrinhos.ListarIdsComReservaVencidaAsync(timeProvider.AgoraUtc(), TamanhoDoLote, cancellationToken);

    /// <returns><see langword="true"/> se foi este chamado que expirou o carrinho.</returns>
    public async Task<bool> ExpirarAsync(Guid carrinhoId, CancellationToken cancellationToken)
    {
        await unitOfWork.IniciarTransacaoAsync(cancellationToken);
        await produtos.BloquearParaAlterarEstoqueAsync(carrinhoId, produtoAdicional: null, cancellationToken);

        var carrinho = await carrinhos.ObterAsync(carrinhoId, cancellationToken);

        // Entre listar e chegar aqui, alguém pode ter mexido na sacola e renovado o prazo: aí não há o que fazer.
        if (carrinho is null || !carrinho.ExpirarSeVencido(timeProvider.AgoraUtc()))
        {
            return false;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return true;
    }
}

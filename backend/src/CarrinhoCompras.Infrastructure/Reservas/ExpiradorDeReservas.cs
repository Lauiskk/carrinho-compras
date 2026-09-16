using CarrinhoCompras.Application.Carrinhos.ExpirarReservas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarrinhoCompras.Infrastructure.Reservas;

/// <summary>
/// Passa de tempos em tempos devolvendo à loja as unidades de sacolas abandonadas.
/// <para>
/// Fica aqui, e não na Application, porque temporizador e ciclo de vida do host são detalhes de execução:
/// o que fazer em cada passagem é decidido por <see cref="ExpirarReservasVencidasHandler"/>.
/// </para>
/// </summary>
internal sealed class ExpiradorDeReservas(
    IServiceScopeFactory escopos,
    TimeProvider relogio,
    ILogger<ExpiradorDeReservas> log) : BackgroundService
{
    /// <summary>De quanto em quanto tempo o serviço procura sacolas vencidas.</summary>
    public static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var temporizador = new PeriodicTimer(Intervalo, relogio);

        while (await temporizador.WaitForNextTickAsync(stoppingToken))
        {
            await UmaPassagemAsync(stoppingToken);
        }
    }

    private async Task UmaPassagemAsync(CancellationToken stoppingToken)
    {
        IReadOnlyList<Guid> vencidos;
        try
        {
            vencidos = await ComEscopoAsync(expirar => expirar.ListarVencidosAsync(stoppingToken));
        }
        catch (Exception excecao) when (NaoEhParada(excecao, stoppingToken))
        {
            log.LogError(excecao, "Falha ao listar as sacolas vencidas; tentando de novo na próxima passagem.");
            return;
        }

        var expirados = 0;
        foreach (var carrinhoId in vencidos)
        {
            try
            {
                // Um escopo (e uma transação) por carrinho, com o erro tratado aqui dentro: uma sacola que
                // falha — por conflito com quem a está usando agora, ou por qualquer outro motivo — não pode
                // levar junto as outras do lote. Como a lista vem ordenada pela mais antiga, sem isto um
                // carrinho problemático ficaria eternamente na frente, segurando o estoque de todos atrás.
                if (await ComEscopoAsync(expirar => expirar.ExpirarAsync(carrinhoId, stoppingToken)))
                {
                    expirados += 1;
                }
            }
            catch (Exception excecao) when (NaoEhParada(excecao, stoppingToken))
            {
                log.LogError(excecao, "Falha ao expirar a sacola {CarrinhoId}; as outras seguem.", carrinhoId);
            }
        }

        if (expirados > 0)
        {
            log.LogInformation("{Quantidade} sacola(s) expiraram e devolveram as unidades à loja.", expirados);
        }
    }

    /// <summary>
    /// Distingue "a aplicação está parando" (que deve subir e encerrar o serviço) de qualquer outra falha
    /// (que é registrada e não derruba a varredura).
    /// </summary>
    private static bool NaoEhParada(Exception excecao, CancellationToken stoppingToken) =>
        excecao is not OperationCanceledException || !stoppingToken.IsCancellationRequested;

    private async Task<T> ComEscopoAsync<T>(Func<ExpirarReservasVencidasHandler, Task<T>> acao)
    {
        await using var escopo = escopos.CreateAsyncScope();
        return await acao(escopo.ServiceProvider.GetRequiredService<ExpirarReservasVencidasHandler>());
    }
}

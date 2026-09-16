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
            try
            {
                var vencidos = await ComEscopoAsync(
                    expirar => expirar.ListarVencidosAsync(stoppingToken));

                var expirados = 0;
                foreach (var carrinhoId in vencidos)
                {
                    // Um escopo (e uma transação) por carrinho: um conflito com quem estiver usando aquela
                    // sacola agora não impede as outras de serem liberadas.
                    if (await ComEscopoAsync(expirar => expirar.ExpirarAsync(carrinhoId, stoppingToken)))
                    {
                        expirados += 1;
                    }
                }

                if (expirados > 0)
                {
                    log.LogInformation("{Quantidade} sacola(s) expiraram e devolveram as unidades à loja.", expirados);
                }
            }
#pragma warning disable CA1031 // Uma falha num carrinho não pode parar a varredura dos outros.
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // A aplicação está parando: sai do laço sem registrar erro.
                break;
            }
            catch (Exception excecao)
#pragma warning restore CA1031
            {
                log.LogError(excecao, "Falha ao expirar as reservas vencidas; tentando de novo na próxima passagem.");
            }
        }
    }

    private async Task<T> ComEscopoAsync<T>(Func<ExpirarReservasVencidasHandler, Task<T>> acao)
    {
        await using var escopo = escopos.CreateAsyncScope();
        return await acao(escopo.ServiceProvider.GetRequiredService<ExpirarReservasVencidasHandler>());
    }
}

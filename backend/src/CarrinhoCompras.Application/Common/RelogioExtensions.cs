namespace CarrinhoCompras.Application.Common;

internal static class RelogioExtensions
{
    /// <summary>
    /// Hora atual em UTC, truncada em microssegundos — a precisão das datas gravadas no banco.
    /// Assim a data devolvida logo após uma alteração é idêntica à lida do banco depois.
    /// </summary>
    public static DateTimeOffset AgoraUtc(this TimeProvider relogio)
    {
        var agora = relogio.GetUtcNow();
        return agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerMicrosecond));
    }
}

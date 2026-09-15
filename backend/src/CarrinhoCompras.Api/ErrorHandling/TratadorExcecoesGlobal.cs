using CarrinhoCompras.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarrinhoCompras.Api.ErrorHandling;

/// <summary>
/// Converte exceções em ProblemDetails. Falhas de negócio não chegam aqui (viajam como <c>Result</c>);
/// este tratador cobre conflitos de concorrência, requisições malformadas e erros inesperados — sem vazar detalhes internos.
/// </summary>
internal sealed partial class TratadorExcecoesGlobal(
    IProblemDetailsService problemDetailsService,
    ILogger<TratadorExcecoesGlobal> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, codigo, detalhe) = exception switch
        {
            ConflitoConcorrenciaException conflito =>
                (StatusCodes.Status409Conflict, "carrinho.conflito_concorrencia", conflito.Message),
            BadHttpRequestException requisicaoInvalida =>
                (requisicaoInvalida.StatusCode, CodigosErroHttp.Codigo(requisicaoInvalida.StatusCode),
                    CodigosErroHttp.Detalhe(requisicaoInvalida.StatusCode)),
            _ =>
                (StatusCodes.Status500InternalServerError, CodigosErroHttp.Codigo(StatusCodes.Status500InternalServerError),
                    CodigosErroHttp.Detalhe(StatusCodes.Status500InternalServerError)),
        };

        // No .NET 10 o framework não registra a exceção quando um IExceptionHandler a trata; o log é responsabilidade daqui.
        if (status >= StatusCodes.Status500InternalServerError)
        {
            LogErroInesperado(logger, httpContext.Request.Method, httpContext.Request.Path, exception);
        }
        else
        {
            LogFalhaTratada(logger, httpContext.Request.Method, httpContext.Request.Path, codigo);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Detail = detalhe,
                Extensions = { ["code"] = codigo },
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro inesperado ao processar {Metodo} {Caminho}")]
    private static partial void LogErroInesperado(ILogger logger, string metodo, string caminho, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Requisição {Metodo} {Caminho} rejeitada: {Codigo}")]
    private static partial void LogFalhaTratada(ILogger logger, string metodo, string caminho, string codigo);
}

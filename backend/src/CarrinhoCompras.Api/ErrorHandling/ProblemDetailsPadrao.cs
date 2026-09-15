using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace CarrinhoCompras.Api.ErrorHandling;

internal static class ProblemDetailsPadrao
{
    /// <summary>
    /// Ponto único de padronização, chamado para todo ProblemDetails gerado na aplicação (controllers,
    /// validação, exceções e páginas de status): título em português, detalhe, instância e <c>code</c>.
    /// O <c>traceId</c> é adicionado pelo próprio ASP.NET Core.
    /// </summary>
    public static void Aplicar(ProblemDetailsContext contexto)
    {
        var problema = contexto.ProblemDetails;
        var status = problema.Status ?? contexto.HttpContext.Response.StatusCode;

        problema.Status = status;
        problema.Title = CodigosErroHttp.Titulo(status);
        problema.Detail ??= CodigosErroHttp.Detalhe(status);
        problema.Instance ??= contexto.HttpContext.Request.Path;
        problema.Extensions.TryAdd("code", CodigosErroHttp.Codigo(status));
    }

    /// <summary>Mensagens de model binding (tipos inválidos, corpo ausente etc.) em português.</summary>
    public static void TraduzirMensagensDeBinding(DefaultModelBindingMessageProvider mensagens)
    {
        mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para '{campo}'.");
        mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        mensagens.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado não é válido para '{campo}'.");
        mensagens.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
        mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
        mensagens.SetValueMustNotBeNullAccessor(campo => $"O campo '{campo}' não pode ser nulo.");
        mensagens.SetValueMustBeANumberAccessor(campo => $"O campo '{campo}' deve ser um número.");
        mensagens.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
        mensagens.SetMissingBindRequiredValueAccessor(campo => $"O campo '{campo}' é obrigatório.");
        mensagens.SetMissingKeyOrValueAccessor(() => "Um valor é obrigatório.");
        mensagens.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição é obrigatório.");
    }
}

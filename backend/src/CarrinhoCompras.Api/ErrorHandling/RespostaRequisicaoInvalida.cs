using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CarrinhoCompras.Api.ErrorHandling;

/// <summary>
/// Resposta 400 única para qualquer entrada inválida: JSON malformado, tipo errado, corpo ausente,
/// parâmetro de rota inválido ou regra do FluentValidation. Os campos aparecem em camelCase, como no JSON.
/// </summary>
internal static class RespostaRequisicaoInvalida
{
    private const string CampoCorpo = "corpo";
    private const string MensagemGenerica = "Valor ausente, malformado ou de tipo inválido.";

    public static IActionResult Criar(ActionContext contexto)
    {
        var erros = new ModelStateDictionary();
        foreach (var (chave, entrada) in contexto.ModelState)
        {
            foreach (var erro in entrada.Errors)
            {
                var mensagem = string.IsNullOrWhiteSpace(erro.ErrorMessage) ? MensagemGenerica : erro.ErrorMessage;
                erros.AddModelError(NormalizarCampo(chave), mensagem);
            }
        }

        var fabrica = contexto.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problema = fabrica.CreateValidationProblemDetails(
            contexto.HttpContext,
            erros,
            StatusCodes.Status400BadRequest,
            detail: "Um ou mais campos da requisição são inválidos.");
        problema.Extensions["code"] = CodigosErroHttp.RequisicaoInvalida;

        return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
    }

    // "$.produtoId" (caminho JSON) → "produtoId"; "" ou "$" (o corpo inteiro) → "corpo".
    private static string NormalizarCampo(string chave)
    {
        var campo = chave.StartsWith("$.", StringComparison.Ordinal) ? chave[2..] : chave.TrimStart('$');
        return string.IsNullOrEmpty(campo) ? CampoCorpo : JsonNamingPolicy.CamelCase.ConvertName(campo);
    }
}

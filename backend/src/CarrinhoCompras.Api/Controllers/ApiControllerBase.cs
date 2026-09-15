using CarrinhoCompras.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace CarrinhoCompras.Api.Controllers;

/// <summary>
/// Base dos controllers: traduz o <see cref="Result{TValue}"/> dos casos de uso para HTTP.
/// É o único lugar que conhece a correspondência entre tipo de erro do domínio e status HTTP.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<TValue> Responder<TValue>(Result<TValue> resultado) =>
        resultado.IsSuccess ? Ok(resultado.Value) : Problema(resultado.Error);

    protected ObjectResult Problema(Error erro)
    {
        ArgumentNullException.ThrowIfNull(erro);

        var status = erro.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problema = ProblemDetailsFactory.CreateProblemDetails(HttpContext, status, detail: erro.Message);
        problema.Extensions["code"] = erro.Code;

        return new ObjectResult(problema)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }
}

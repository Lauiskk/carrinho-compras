using CarrinhoCompras.Application.Carrinhos;
using CarrinhoCompras.Application.Carrinhos.AplicarCupom;
using CarrinhoCompras.Application.Carrinhos.RemoverCupom;
using Microsoft.AspNetCore.Mvc;

namespace CarrinhoCompras.Api.Controllers;

[Route("api/carrinhos/{carrinhoId}/cupom")]
[Tags("Cupom do carrinho")]
public sealed class CarrinhoCupomController : ApiControllerBase
{
    /// <summary>Aplica um cupom de desconto ao carrinho.</summary>
    /// <remarks>
    /// Só existe um cupom ativo por vez: aplicar outro substitui o anterior. O desconto incide sobre o subtotal.
    /// </remarks>
    /// <param name="carrinhoId">Identificador do carrinho.</param>
    /// <param name="request">Código do cupom.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Carrinho com o cupom aplicado e desconto e total recalculados.</response>
    /// <response code="400">Código não informado.</response>
    /// <response code="404">Carrinho não existe.</response>
    /// <response code="409">Carrinho já finalizado.</response>
    /// <response code="422">Cupom inválido ou inexistente.</response>
    [HttpPut]
    [Consumes("application/json")]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<ActionResult<CarrinhoResponse>> Aplicar(
        Guid carrinhoId,
        AplicarCupomRequest request,
        [FromServices] AplicarCupomHandler handler,
        CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(carrinhoId, request, cancellationToken));

    /// <summary>Remove o cupom aplicado.</summary>
    /// <remarks>Operação idempotente: se não houver cupom aplicado, o carrinho é devolvido sem alteração.</remarks>
    /// <param name="carrinhoId">Identificador do carrinho.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Carrinho sem cupom, com desconto zerado e total recalculado.</response>
    /// <response code="400">Identificador inválido.</response>
    /// <response code="404">Carrinho não existe.</response>
    /// <response code="409">Carrinho já finalizado.</response>
    [HttpDelete]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<CarrinhoResponse>> Remover(
        Guid carrinhoId, [FromServices] RemoverCupomHandler handler, CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(carrinhoId, cancellationToken));
}

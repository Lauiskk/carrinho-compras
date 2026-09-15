using CarrinhoCompras.Application.Carrinhos;
using CarrinhoCompras.Application.Carrinhos.CriarCarrinho;
using CarrinhoCompras.Application.Carrinhos.FinalizarCarrinho;
using CarrinhoCompras.Application.Carrinhos.ObterCarrinho;
using Microsoft.AspNetCore.Mvc;

namespace CarrinhoCompras.Api.Controllers;

[Route("api/carrinhos")]
[Tags("Carrinhos")]
public sealed class CarrinhosController : ApiControllerBase
{
    /// <summary>Cria um carrinho vazio.</summary>
    /// <remarks>O carrinho nasce com status Aberto e subtotal, desconto e total zerados.</remarks>
    /// <response code="201">Carrinho criado; o cabeçalho Location aponta para ele.</response>
    [HttpPost]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status201Created, "application/json")]
    public async Task<ActionResult<CarrinhoResponse>> Criar(
        [FromServices] CriarCarrinhoHandler handler, CancellationToken cancellationToken)
    {
        var carrinho = await handler.HandleAsync(cancellationToken);
        return CreatedAtAction(nameof(Obter), new { carrinhoId = carrinho.Id }, carrinho);
    }

    /// <summary>Obtém o carrinho com itens, cupom e valores calculados.</summary>
    /// <param name="carrinhoId">Identificador do carrinho.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Carrinho encontrado.</response>
    /// <response code="400">Identificador inválido.</response>
    /// <response code="404">Carrinho não existe.</response>
    [HttpGet("{carrinhoId}")]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<CarrinhoResponse>> Obter(
        Guid carrinhoId, [FromServices] ObterCarrinhoHandler handler, CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(carrinhoId, cancellationToken));

    /// <summary>Finaliza o carrinho (checkout).</summary>
    /// <remarks>Depois de finalizado, o carrinho não aceita mais alterações de itens, quantidades ou cupom.</remarks>
    /// <param name="carrinhoId">Identificador do carrinho.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Carrinho finalizado.</response>
    /// <response code="404">Carrinho não existe.</response>
    /// <response code="409">O carrinho já estava finalizado.</response>
    /// <response code="422">Carrinho sem itens não pode ser finalizado.</response>
    [HttpPost("{carrinhoId}/finalizar")]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<ActionResult<CarrinhoResponse>> Finalizar(
        Guid carrinhoId, [FromServices] FinalizarCarrinhoHandler handler, CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(carrinhoId, cancellationToken));
}

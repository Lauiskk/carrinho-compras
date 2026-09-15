using CarrinhoCompras.Application.Carrinhos;
using CarrinhoCompras.Application.Carrinhos.AdicionarItem;
using CarrinhoCompras.Application.Carrinhos.AlterarQuantidadeItem;
using CarrinhoCompras.Application.Carrinhos.RemoverItem;
using Microsoft.AspNetCore.Mvc;

namespace CarrinhoCompras.Api.Controllers;

[Route("api/carrinhos/{carrinhoId}/itens")]
[Tags("Itens do carrinho")]
public sealed class CarrinhoItensController : ApiControllerBase
{
    /// <summary>Adiciona um produto ao carrinho.</summary>
    /// <remarks>
    /// Se o produto ainda não está no carrinho, entra com a quantidade informada (padrão 1).
    /// Se já está, a quantidade é somada à existente. A quantidade resultante não pode passar do estoque.
    /// </remarks>
    /// <param name="carrinhoId">Identificador do carrinho.</param>
    /// <param name="request">Produto e quantidade.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Carrinho com o preço do item, subtotal, desconto e total recalculados.</response>
    /// <response code="400">Dados inválidos (ex.: quantidade menor ou igual a zero).</response>
    /// <response code="404">Carrinho ou produto não existe.</response>
    /// <response code="409">Carrinho já finalizado.</response>
    /// <response code="422">Estoque insuficiente.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<ActionResult<CarrinhoResponse>> Adicionar(
        Guid carrinhoId,
        AdicionarItemRequest request,
        [FromServices] AdicionarItemHandler handler,
        CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(carrinhoId, request, cancellationToken));

    /// <summary>Altera a quantidade de um produto que já está no carrinho.</summary>
    /// <remarks>Substitui a quantidade atual pela informada (pode aumentar ou diminuir), respeitando o estoque.</remarks>
    /// <param name="carrinhoId">Identificador do carrinho.</param>
    /// <param name="produtoId">Identificador do produto no carrinho.</param>
    /// <param name="request">Nova quantidade.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Carrinho com o preço do item, subtotal, desconto e total recalculados.</response>
    /// <response code="400">Dados inválidos (ex.: quantidade menor ou igual a zero).</response>
    /// <response code="404">Carrinho não existe ou o produto não está no carrinho.</response>
    /// <response code="409">Carrinho já finalizado.</response>
    /// <response code="422">Estoque insuficiente.</response>
    [HttpPut("{produtoId}")]
    [Consumes("application/json")]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<ActionResult<CarrinhoResponse>> AlterarQuantidade(
        Guid carrinhoId,
        int produtoId,
        AlterarQuantidadeItemRequest request,
        [FromServices] AlterarQuantidadeItemHandler handler,
        CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(carrinhoId, produtoId, request, cancellationToken));

    /// <summary>Remove um produto do carrinho.</summary>
    /// <param name="carrinhoId">Identificador do carrinho.</param>
    /// <param name="produtoId">Identificador do produto no carrinho.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Carrinho sem o produto, com subtotal, desconto e total recalculados.</response>
    /// <response code="400">Identificador inválido.</response>
    /// <response code="404">Carrinho não existe ou o produto não está no carrinho.</response>
    /// <response code="409">Carrinho já finalizado.</response>
    [HttpDelete("{produtoId}")]
    [ProducesResponseType<CarrinhoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<CarrinhoResponse>> Remover(
        Guid carrinhoId,
        int produtoId,
        [FromServices] RemoverItemHandler handler,
        CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(carrinhoId, produtoId, cancellationToken));
}

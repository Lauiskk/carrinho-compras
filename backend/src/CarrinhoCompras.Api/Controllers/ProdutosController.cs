using CarrinhoCompras.Application.Produtos;
using CarrinhoCompras.Application.Produtos.ListarProdutos;
using CarrinhoCompras.Application.Produtos.ObterProduto;
using Microsoft.AspNetCore.Mvc;

namespace CarrinhoCompras.Api.Controllers;

[Route("api/produtos")]
[Tags("Produtos")]
public sealed class ProdutosController : ApiControllerBase
{
    /// <summary>Lista o catálogo de produtos.</summary>
    /// <remarks>Cada produto expõe o preço líquido unitário e a quantidade disponível em estoque.</remarks>
    /// <response code="200">Produtos do catálogo, ordenados pelo id.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProdutoResponse>>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult<IReadOnlyList<ProdutoResponse>>> Listar(
        [FromServices] ListarProdutosHandler handler, CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(cancellationToken));

    /// <summary>Obtém um produto do catálogo.</summary>
    /// <param name="produtoId">Identificador do produto.</param>
    /// <param name="handler">Caso de uso (injetado).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Produto encontrado.</response>
    /// <response code="400">Identificador inválido.</response>
    /// <response code="404">Produto não existe no catálogo.</response>
    [HttpGet("{produtoId}")]
    [ProducesResponseType<ProdutoResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProdutoResponse>> Obter(
        int produtoId, [FromServices] ObterProdutoHandler handler, CancellationToken cancellationToken) =>
        Responder(await handler.HandleAsync(produtoId, cancellationToken));
}

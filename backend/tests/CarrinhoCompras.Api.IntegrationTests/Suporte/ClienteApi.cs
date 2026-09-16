using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarrinhoCompras.Application.Carrinhos;
using CarrinhoCompras.Application.Produtos;

namespace CarrinhoCompras.Api.IntegrationTests.Suporte;

/// <summary>Atalhos para chamar a API nos testes, sempre pelo HTTP (como um cliente real).</summary>
internal static class ClienteApi
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    public static async Task<CarrinhoResponse> CriarCarrinhoAsync(this HttpClient cliente)
    {
        var resposta = await cliente.PostAsync("/api/carrinhos", content: null, Cancelamento);
        resposta.StatusCode.ShouldBe(HttpStatusCode.Created);
        return await resposta.LerAsync<CarrinhoResponse>();
    }

    public static Task<HttpResponseMessage> ObterCarrinhoAsync(this HttpClient cliente, Guid carrinhoId) =>
        cliente.GetAsync($"/api/carrinhos/{carrinhoId}", Cancelamento);

    public static async Task<CarrinhoResponse> ObterCarrinhoComSucessoAsync(this HttpClient cliente, Guid carrinhoId)
    {
        var resposta = await cliente.ObterCarrinhoAsync(carrinhoId);
        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await resposta.LerAsync<CarrinhoResponse>();
    }

    public static Task<HttpResponseMessage> AdicionarItemAsync(
        this HttpClient cliente, Guid carrinhoId, int produtoId, int? quantidade = null)
    {
        // Sem quantidade, o campo nem é enviado: exercita o valor padrão (1) da API.
        object corpo = quantidade is null ? new { produtoId } : new { produtoId, quantidade };
        return cliente.PostAsJsonAsync($"/api/carrinhos/{carrinhoId}/itens", corpo, Json, Cancelamento);
    }

    public static async Task<CarrinhoResponse> AdicionarItemComSucessoAsync(
        this HttpClient cliente, Guid carrinhoId, int produtoId, int? quantidade = null)
    {
        var resposta = await cliente.AdicionarItemAsync(carrinhoId, produtoId, quantidade);
        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await resposta.LerAsync<CarrinhoResponse>();
    }

    public static Task<HttpResponseMessage> AlterarQuantidadeAsync(this HttpClient cliente, Guid carrinhoId, int produtoId, int quantidade) =>
        cliente.PutAsJsonAsync($"/api/carrinhos/{carrinhoId}/itens/{produtoId}", new { quantidade }, Json, Cancelamento);

    public static Task<HttpResponseMessage> RemoverItemAsync(this HttpClient cliente, Guid carrinhoId, int produtoId) =>
        cliente.DeleteAsync($"/api/carrinhos/{carrinhoId}/itens/{produtoId}", Cancelamento);

    public static Task<HttpResponseMessage> AplicarCupomAsync(this HttpClient cliente, Guid carrinhoId, string? codigoCupom) =>
        cliente.PutAsJsonAsync($"/api/carrinhos/{carrinhoId}/cupom", new { codigoCupom }, Json, Cancelamento);

    public static Task<HttpResponseMessage> RemoverCupomAsync(this HttpClient cliente, Guid carrinhoId) =>
        cliente.DeleteAsync($"/api/carrinhos/{carrinhoId}/cupom", Cancelamento);

    public static Task<HttpResponseMessage> FinalizarAsync(this HttpClient cliente, Guid carrinhoId) =>
        cliente.PostAsync($"/api/carrinhos/{carrinhoId}/finalizar", content: null, Cancelamento);

    /// <summary>
    /// Escolhe um produto olhando a <b>disponibilidade atual</b> da API, não os números do JSON: com reserva
    /// de estoque esse valor muda enquanto os testes rodam. Pega o de maior folga para reduzir disputa.
    /// </summary>
    public static async Task<ProdutoResponse> ProdutoComDisponivelAsync(
        this HttpClient cliente, int minimo, params int[] exceto)
    {
        var produtos = await cliente.ListarProdutosAsync();
        return produtos
            .Where(produto => produto.QuantidadeDisponivel >= minimo && !exceto.Contains(produto.Id))
            .MaxBy(produto => produto.QuantidadeDisponivel)
            ?? throw new InvalidOperationException(
                $"Nenhum produto do catálogo tem {minimo} unidade(s) disponível(is) neste momento.");
    }

    /// <summary>Estado atual de um produto no catálogo (estoque, reservado e disponível).</summary>
    public static async Task<ProdutoResponse> ProdutoAtualAsync(this HttpClient cliente, int produtoId) =>
        (await cliente.ListarProdutosAsync()).Single(produto => produto.Id == produtoId);

    public static async Task<IReadOnlyList<ProdutoResponse>> ListarProdutosAsync(this HttpClient cliente) =>
        await cliente.GetFromJsonAsync<List<ProdutoResponse>>("/api/produtos", Json, Cancelamento)
        ?? throw new InvalidOperationException("A listagem de produtos veio vazia.");

    public static async Task<T> LerAsync<T>(this HttpResponseMessage resposta) =>
        await resposta.Content.ReadFromJsonAsync<T>(Json, Cancelamento)
        ?? throw new InvalidOperationException($"Corpo vazio ao ler {typeof(T).Name}.");

    public static async Task<T> DeveTerSucessoAsync<T>(this HttpResponseMessage resposta)
    {
        if (resposta.StatusCode != HttpStatusCode.OK)
        {
            var corpo = await resposta.Content.ReadAsStringAsync(Cancelamento);
            throw new ShouldAssertException($"Esperava 200 OK, mas veio {(int)resposta.StatusCode}: {corpo}");
        }

        return await resposta.LerAsync<T>();
    }
}

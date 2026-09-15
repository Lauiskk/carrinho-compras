using System.Net;
using CarrinhoCompras.Api.IntegrationTests.Suporte;

namespace CarrinhoCompras.Api.IntegrationTests.Concorrencia;

/// <summary>
/// Concorrência otimista: requisições simultâneas no mesmo carrinho nunca perdem atualizações nem viram erro 500.
/// Quem perde a corrida recebe 409 e pode recarregar o carrinho.
/// </summary>
public sealed class ConcorrenciaTests(ApiFactory api)
{
    private const int RequisicoesSimultaneas = 6;

    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task Adicoes_simultaneas_ao_mesmo_item_nao_perdem_atualizacoes()
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(RequisicoesSimultaneas + 1);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 1);

        var respostas = await Task.WhenAll(Enumerable.Range(0, RequisicoesSimultaneas)
            .Select(_ => _cliente.AdicionarItemAsync(carrinho.Id, produto.Id, quantidade: 1)));

        var sucessos = await ConferirRespostasAsync(respostas);
        var final = await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id);
        final.Itens.ShouldHaveSingleItem().Quantidade.ShouldBe(1 + sucessos);
    }

    [Fact]
    public async Task Primeira_adicao_simultanea_do_mesmo_produto_gera_uma_unica_linha()
    {
        var produto = Suporte.Catalogo.ComEstoqueDePeloMenos(RequisicoesSimultaneas);
        var carrinho = await _cliente.CriarCarrinhoAsync();

        var respostas = await Task.WhenAll(Enumerable.Range(0, RequisicoesSimultaneas)
            .Select(_ => _cliente.AdicionarItemAsync(carrinho.Id, produto.Id, quantidade: 1)));

        var sucessos = await ConferirRespostasAsync(respostas);
        var final = await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id);
        final.Itens.ShouldHaveSingleItem().Quantidade.ShouldBe(sucessos);
    }

    private static async Task<int> ConferirRespostasAsync(HttpResponseMessage[] respostas)
    {
        foreach (var conflito in respostas.Where(resposta => resposta.StatusCode != HttpStatusCode.OK))
        {
            await conflito.DeveSerProblemaAsync(HttpStatusCode.Conflict, "carrinho.conflito_concorrencia");
        }

        var sucessos = respostas.Count(resposta => resposta.StatusCode == HttpStatusCode.OK);
        sucessos.ShouldBeGreaterThanOrEqualTo(1);
        return sucessos;
    }
}

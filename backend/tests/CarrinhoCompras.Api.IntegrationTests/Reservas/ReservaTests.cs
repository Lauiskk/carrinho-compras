using System.Net;
using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Application.Carrinhos;
using CarrinhoCompras.Domain.Carrinhos;
using Npgsql;

namespace CarrinhoCompras.Api.IntegrationTests.Reservas;

/// <summary>
/// Reserva de estoque contra a API e o PostgreSQL de verdade: o que uma sacola segura some da vitrine para
/// as outras, volta quando ela solta, e vira baixa definitiva no checkout.
/// </summary>
public sealed class ReservaTests(ApiFactory api)
{
    private readonly HttpClient _cliente = api.CreateClient();

    [Fact]
    public async Task O_que_entra_na_sacola_sai_da_vitrine_e_volta_ao_sair()
    {
        var produto = await _cliente.ProdutoComDisponivelAsync(3);
        var carrinho = await _cliente.CriarCarrinhoAsync();

        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id, quantidade: 3);
        var comReserva = await _cliente.ProdutoAtualAsync(produto.Id);
        comReserva.QuantidadeDisponivel.ShouldBe(produto.QuantidadeDisponivel - 3);
        comReserva.QuantidadeEstoque.ShouldBe(produto.QuantidadeEstoque);

        // Diminuir devolve só a diferença.
        await (await _cliente.AlterarQuantidadeAsync(carrinho.Id, produto.Id, 1)).DeveTerSucessoAsync<CarrinhoResponse>();
        (await _cliente.ProdutoAtualAsync(produto.Id)).QuantidadeDisponivel.ShouldBe(produto.QuantidadeDisponivel - 1);

        // Remover devolve o resto.
        await (await _cliente.RemoverItemAsync(carrinho.Id, produto.Id)).DeveTerSucessoAsync<CarrinhoResponse>();
        (await _cliente.ProdutoAtualAsync(produto.Id)).QuantidadeDisponivel.ShouldBe(produto.QuantidadeDisponivel);
    }

    /// <summary>
    /// O caso que a reserva existe para resolver: sem ela, as duas sacolas levariam a mesma última unidade.
    /// </summary>
    [Fact]
    public async Task Duas_sacolas_nao_levam_a_mesma_ultima_unidade()
    {
        var produto = await _cliente.ProdutoComDisponivelAsync(1);
        var primeira = await _cliente.CriarCarrinhoAsync();
        var segunda = await _cliente.CriarCarrinhoAsync();

        // A primeira leva tudo o que estava disponível.
        await _cliente.AdicionarItemComSucessoAsync(primeira.Id, produto.Id, produto.QuantidadeDisponivel);

        var recusa = await _cliente.AdicionarItemAsync(segunda.Id, produto.Id, quantidade: 1);

        var problema = await recusa.DeveSerProblemaAsync(HttpStatusCode.UnprocessableEntity, "produto.estoque_insuficiente");
        problema.Detalhe().ShouldContain("0 disponível(is)");
        (await _cliente.ObterCarrinhoComSucessoAsync(segunda.Id)).Itens.ShouldBeEmpty();
    }

    /// <summary>
    /// Com bloqueio da linha do produto, requisições simultâneas de sacolas diferentes <b>esperam</b> em vez
    /// de falhar — e o total reservado nunca passa do estoque.
    /// </summary>
    [Fact]
    public async Task Sacolas_simultaneas_nunca_reservam_mais_do_que_existe()
    {
        const int Sacolas = 6;
        var produto = await _cliente.ProdutoComDisponivelAsync(Sacolas);
        var disponivelAntes = produto.QuantidadeDisponivel;
        var carrinhos = await Task.WhenAll(Enumerable.Range(0, Sacolas).Select(_ => _cliente.CriarCarrinhoAsync()));

        var respostas = await Task.WhenAll(
            carrinhos.Select(carrinho => _cliente.AdicionarItemAsync(carrinho.Id, produto.Id, quantidade: 1)));

        var aceitas = respostas.Count(resposta => resposta.StatusCode == HttpStatusCode.OK);
        aceitas.ShouldBe(Sacolas, "com o bloqueio da linha, nenhuma sacola deveria ser recusada por conflito");

        var depois = await _cliente.ProdutoAtualAsync(produto.Id);
        depois.QuantidadeReservada.ShouldBe(produto.QuantidadeReservada + Sacolas);
        depois.QuantidadeDisponivel.ShouldBe(disponivelAntes - Sacolas);
        depois.QuantidadeReservada.ShouldBeLessThanOrEqualTo(depois.QuantidadeEstoque);
    }

    /// <summary>
    /// A sacola vence e a pessoa tenta alterá-la. A operação é recusada — mas as unidades <b>precisam</b>
    /// voltar para a vitrine nesse mesmo momento, gravadas: quem descobre o vencimento é justamente a
    /// operação recusada. Sem isso, o estoque ficaria preso até a varredura em segundo plano passar, e a
    /// consulta seguinte ainda mostraria a sacola aberta com um prazo no passado.
    /// </summary>
    [Fact]
    public async Task Alterar_uma_sacola_vencida_devolve_as_unidades_de_verdade()
    {
        var produto = await _cliente.ProdutoComDisponivelAsync(1);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id);
        (await _cliente.ProdutoAtualAsync(produto.Id)).QuantidadeDisponivel.ShouldBe(produto.QuantidadeDisponivel - 1);

        await VencerAReservaAsync(carrinho.Id);

        var recusa = await _cliente.AdicionarItemAsync(carrinho.Id, produto.Id);

        await recusa.DeveSerProblemaAsync(HttpStatusCode.Conflict, "carrinho.expirado");
        (await _cliente.ProdutoAtualAsync(produto.Id)).QuantidadeDisponivel
            .ShouldBe(produto.QuantidadeDisponivel, "a recusa não pode descartar a devolução das unidades");
        (await _cliente.ObterCarrinhoComSucessoAsync(carrinho.Id)).Status.ShouldBe(StatusCarrinho.Expirado);
    }

    /// <summary>Empurra o prazo para o passado direto no banco — mais rápido e determinístico que esperar.</summary>
    private async Task VencerAReservaAsync(Guid carrinhoId)
    {
        await using var conexao = new NpgsqlConnection(api.ConnectionString);
        await conexao.OpenAsync(TestContext.Current.CancellationToken);
        await using var comando = new NpgsqlCommand(
            """UPDATE "Carrinho" SET "ExpiraEm" = now() - interval '1 hour' WHERE "ID" = @id""", conexao);
        comando.Parameters.AddWithValue("id", carrinhoId);
        (await comando.ExecuteNonQueryAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task O_carrinho_informa_ate_quando_a_reserva_vale()
    {
        var produto = await _cliente.ProdutoComDisponivelAsync(1);
        var carrinho = await _cliente.CriarCarrinhoAsync();
        carrinho.ExpiraEm.ShouldBeNull("uma sacola vazia não segura nada, então não tem prazo");

        var comItem = await _cliente.AdicionarItemComSucessoAsync(carrinho.Id, produto.Id);

        var prazo = comItem.ExpiraEm.ShouldNotBeNull();
        prazo.ShouldBeGreaterThan(comItem.CriadoEm);

        // Esvaziar solta as unidades e, com elas, o prazo.
        var vazio = await (await _cliente.RemoverItemAsync(carrinho.Id, produto.Id)).DeveTerSucessoAsync<CarrinhoResponse>();
        vazio.ExpiraEm.ShouldBeNull();
    }
}

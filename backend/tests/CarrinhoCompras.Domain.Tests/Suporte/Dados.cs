using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Common;
using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;

namespace CarrinhoCompras.Domain.Tests.Suporte;

/// <summary>Fábricas de dados de teste: cada teste declara só o que importa para o cenário.</summary>
internal static class Dados
{
    public static readonly DateTimeOffset Agora = new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

    public static Produto Produto(int id = 1, decimal preco = 10.50m, int estoque = 100, string? descricao = null) =>
        new(id, descricao ?? $"Produto {id}", preco, estoque);

    public static Cupom Cupom(decimal percentual, int id = 1, string? codigo = null) =>
        new(id, codigo ?? $"{percentual:0}OFF", percentual);

    public static Carrinho CarrinhoNovo() => Carrinho.Criar(Agora);

    public static Carrinho CarrinhoCom(params (Produto Produto, int Quantidade)[] itens)
    {
        var carrinho = CarrinhoNovo();
        foreach (var (produto, quantidade) in itens)
        {
            carrinho.AdicionarItem(produto, quantidade).DeveTerSucesso();
        }

        return carrinho;
    }

    // ---------------------------------------------------------------------------------------------------
    // Atalhos que usam Agora. A maioria dos testes é sobre as regras, não sobre o relógio; os testes de
    // expiração chamam os métodos do domínio com o instante explícito, que é onde o tempo importa.
    // ---------------------------------------------------------------------------------------------------
    public static Result AdicionarItem(this Carrinho carrinho, Produto produto, int quantidade) =>
        carrinho.AdicionarItem(produto, quantidade, Agora);

    public static Result AlterarQuantidadeItem(this Carrinho carrinho, int produtoId, int quantidade) =>
        carrinho.AlterarQuantidadeItem(produtoId, quantidade, Agora);

    public static Result RemoverItem(this Carrinho carrinho, int produtoId) =>
        carrinho.RemoverItem(produtoId, Agora);

    public static Result AplicarCupom(this Carrinho carrinho, Cupom cupom) =>
        carrinho.AplicarCupom(cupom, Agora);

    public static Result RemoverCupom(this Carrinho carrinho) =>
        carrinho.RemoverCupom(Agora);

    public static Result VerificarSePodeSerAlterado(this Carrinho carrinho) =>
        carrinho.VerificarSePodeSerAlterado(Agora);

    public static void DeveTerSucesso(this Result resultado) =>
        resultado.IsSuccess.ShouldBeTrue($"Esperava sucesso, mas falhou com: {resultado.Error}");

    public static void DeveFalharCom(this Result resultado, Error esperado)
    {
        resultado.IsFailure.ShouldBeTrue("Esperava falha, mas a operação teve sucesso.");
        resultado.Error.ShouldBe(esperado);
    }

    public static void DeveFalharComCodigo(this Result resultado, string codigo, ErrorType tipo)
    {
        var erro = resultado.Error.ShouldNotBeNull("Esperava falha, mas a operação teve sucesso.");
        erro.Code.ShouldBe(codigo);
        erro.Type.ShouldBe(tipo);
    }

    /// <summary>Retrato do estado observável do carrinho, para provar que uma operação rejeitada não mudou nada.</summary>
    public static string Retrato(this Carrinho carrinho) =>
        $"{carrinho.Status}|cupom:{carrinho.CupomId}|{carrinho.Subtotal}|{carrinho.Desconto}|{carrinho.Total}|" +
        string.Join(",", carrinho.Itens.Select(item => $"{item.ProdutoId}x{item.Quantidade}={item.PrecoItem}")) +
        $"|reservas:{string.Join(",", carrinho.Itens.Select(item => $"{item.ProdutoId}={item.Produto.QuantidadeReservada}"))}";
}

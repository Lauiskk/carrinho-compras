using CarrinhoCompras.Application.Carrinhos.AdicionarItem;
using CarrinhoCompras.Application.Carrinhos.AlterarQuantidadeItem;
using CarrinhoCompras.Application.Carrinhos.AplicarCupom;
using CarrinhoCompras.Application.Carrinhos.CriarCarrinho;
using CarrinhoCompras.Application.Carrinhos.ExpirarReservas;
using CarrinhoCompras.Application.Carrinhos.FinalizarCarrinho;
using CarrinhoCompras.Application.Carrinhos.ObterCarrinho;
using CarrinhoCompras.Application.Carrinhos.RemoverCupom;
using CarrinhoCompras.Application.Carrinhos.RemoverItem;
using CarrinhoCompras.Application.Produtos.ListarProdutos;
using CarrinhoCompras.Application.Produtos.ObterProduto;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarrinhoCompras.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<AdicionarItemRequestValidator>();

        // Um handler por caso de uso, registrado explicitamente: fácil de encontrar e sem "mágica" de reflexão.
        services.AddScoped<ListarProdutosHandler>();
        services.AddScoped<ObterProdutoHandler>();
        services.AddScoped<CriarCarrinhoHandler>();
        services.AddScoped<ObterCarrinhoHandler>();
        services.AddScoped<AdicionarItemHandler>();
        services.AddScoped<AlterarQuantidadeItemHandler>();
        services.AddScoped<RemoverItemHandler>();
        services.AddScoped<AplicarCupomHandler>();
        services.AddScoped<RemoverCupomHandler>();
        services.AddScoped<FinalizarCarrinhoHandler>();
        services.AddScoped<ExpirarReservasVencidasHandler>();

        return services;
    }
}

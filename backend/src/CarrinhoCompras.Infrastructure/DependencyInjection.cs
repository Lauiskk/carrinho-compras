using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Infrastructure.Reservas;
using CarrinhoCompras.Infrastructure.Persistence;
using CarrinhoCompras.Infrastructure.Persistence.Repositories;
using CarrinhoCompras.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarrinhoCompras.Infrastructure;

public static class DependencyInjection
{
    public const string NomeConnectionString = "CarrinhoCompras";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // A connection string é lida só quando o contexto é criado: assim ela pode vir de appsettings,
        // variáveis de ambiente (Docker) ou ser sobrescrita pelos testes de integração.
        services.AddDbContext<CarrinhoComprasDbContext>((provedor, opcoes) =>
        {
            var connectionString = provedor.GetRequiredService<IConfiguration>().GetConnectionString(NomeConnectionString)
                ?? throw new InvalidOperationException(
                    $"A connection string '{NomeConnectionString}' não foi configurada (ConnectionStrings:{NomeConnectionString}).");

            opcoes.UseNpgsql(connectionString)
                .UseSeeding((contexto, _) => CatalogoSeeder.Sincronizar(contexto))
                .UseAsyncSeeding((contexto, _, cancellationToken) => CatalogoSeeder.SincronizarAsync(contexto, cancellationToken));
        });

        services.AddScoped<IUnitOfWork>(provedor => provedor.GetRequiredService<CarrinhoComprasDbContext>());
        services.AddScoped<ICarrinhoRepository, CarrinhoRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<ICupomRepository, CupomRepository>();

        // Devolve à loja as unidades de sacolas abandonadas, sem depender de alguém abrir o carrinho de novo.
        services.AddHostedService<ExpiradorDeReservas>();

        services.AddHealthChecks()
            .AddDbContextCheck<CarrinhoComprasDbContext>("banco-de-dados");

        return services;
    }

    /// <summary>
    /// Aplica as migrations pendentes e sincroniza o catálogo. Pensado para desenvolvimento local; em produção
    /// as migrations rodam antes da aplicação subir (no Docker Compose, pelo serviço <c>migrations</c>).
    /// </summary>
    public static async Task AplicarMigracoesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var escopo = services.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<CarrinhoComprasDbContext>();
        await contexto.Database.MigrateAsync(cancellationToken);
    }
}

using CarrinhoCompras.Api.IntegrationTests.Suporte;
using CarrinhoCompras.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(ApiFactory))]

// Com reserva de estoque, o catálogo virou estado compartilhado: dois testes rodando ao mesmo tempo
// disputam as mesmas unidades e um veria os números do outro. As requisições realmente simultâneas
// continuam existindo dentro de ConcorrenciaTests, que é onde a concorrência é o assunto.
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace CarrinhoCompras.Api.IntegrationTests.Suporte;

/// <summary>
/// Sobe a API de verdade (em memória) contra um PostgreSQL real, compartilhado por todos os testes.
/// Por padrão o banco é um container descartável (Testcontainers). Sem Docker, é possível apontar para um
/// PostgreSQL existente pela variável <see cref="VariavelConnectionString"/> (use um banco dedicado a testes).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string VariavelConnectionString = "TESTES_POSTGRES_CONNECTION_STRING";

    private readonly PostgreSqlContainer? _container;

    public ApiFactory()
    {
        var connectionStringExterna = Environment.GetEnvironmentVariable(VariavelConnectionString);
        if (string.IsNullOrWhiteSpace(connectionStringExterna))
        {
            _container = new PostgreSqlBuilder("postgres:18-alpine").Build();
        }
        else
        {
            ConnectionString = connectionStringExterna;
        }
    }

    public string ConnectionString { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        if (_container is not null)
        {
            await _container.StartAsync(TestContext.Current.CancellationToken);
            ConnectionString = _container.GetConnectionString();
        }

        // Cria o host (com a connection string acima) e aplica migrations + seed uma única vez.
        await Services.AplicarMigracoesAsync(TestContext.Current.CancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Fonte adicionada por último: tem precedência sobre appsettings e variáveis de ambiente.
        builder.ConfigureAppConfiguration((_, configuracao) => configuracao.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{Infrastructure.DependencyInjection.NomeConnectionString}"] = ConnectionString,
                ["Database:AplicarMigracoesNaInicializacao"] = "false",
                ["Documentacao:Habilitada"] = "true",
            }));
    }
}

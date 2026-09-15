using CarrinhoCompras.Application.Common;
using CarrinhoCompras.Domain.Carrinhos;
using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;
using CarrinhoCompras.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CarrinhoCompras.Infrastructure.Persistence;

public sealed class CarrinhoComprasDbContext(DbContextOptions<CarrinhoComprasDbContext> options)
    : DbContext(options), IUnitOfWork
{
    private const string MensagemConflito =
        "O carrinho foi alterado por outra requisição. Recarregue o carrinho e tente novamente.";

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<Cupom> Cupons => Set<Cupom>();

    public DbSet<Carrinho> Carrinhos => Set<Carrinho>();

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        MarcarCarrinhosComItensAlterados();

        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflitoConcorrenciaException(MensagemConflito, ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: ItemCarrinhoConfiguration.ChavePrimaria,
        })
        {
            // Duas requisições adicionando o mesmo produto novo ao mesmo carrinho ao mesmo tempo.
            throw new ConflitoConcorrenciaException(MensagemConflito, ex);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarrinhoComprasDbContext).Assembly);

    /// <summary>
    /// A versão (xmin) fica na linha do carrinho. Quando só os itens mudam, a raiz também é marcada como
    /// modificada para que o UPDATE dela carregue a checagem de concorrência otimista.
    /// </summary>
    private void MarcarCarrinhosComItensAlterados()
    {
        ChangeTracker.DetectChanges();

        var carrinhosComItensAlterados = ChangeTracker.Entries<ItemCarrinho>()
            .Where(entrada => entrada.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entrada => entrada.Entity.CarrinhoId)
            .ToHashSet();

        foreach (var entrada in ChangeTracker.Entries<Carrinho>())
        {
            if (entrada.State == EntityState.Unchanged && carrinhosComItensAlterados.Contains(entrada.Entity.Id))
            {
                entrada.State = EntityState.Modified;
            }
        }
    }
}

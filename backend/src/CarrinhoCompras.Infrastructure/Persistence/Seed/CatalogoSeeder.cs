using CarrinhoCompras.Domain.Cupons;
using CarrinhoCompras.Domain.Produtos;
using Microsoft.EntityFrameworkCore;

namespace CarrinhoCompras.Infrastructure.Persistence.Seed;

/// <summary>
/// Sincroniza o catálogo do banco com os arquivos JSON: insere o que falta e atualiza o que mudou, pelo ID.
/// É idempotente (rodar de novo não duplica nada) e é chamado pelo EF Core ao aplicar as migrations
/// (<c>UseSeeding</c>/<c>UseAsyncSeeding</c>). Linhas que não estão no JSON são mantidas, pois podem estar
/// referenciadas por carrinhos.
/// </summary>
internal static class CatalogoSeeder
{
    public static void Sincronizar(DbContext contexto)
    {
        var produtos = contexto.Set<Produto>().ToDictionary(produto => produto.Id);
        var cupons = contexto.Set<Cupom>().ToDictionary(cupom => cupom.Id);

        if (AplicarAlteracoes(contexto, produtos, cupons))
        {
            contexto.SaveChanges();
        }
    }

    public static async Task SincronizarAsync(DbContext contexto, CancellationToken cancellationToken)
    {
        var produtos = await contexto.Set<Produto>().ToDictionaryAsync(produto => produto.Id, cancellationToken);
        var cupons = await contexto.Set<Cupom>().ToDictionaryAsync(cupom => cupom.Id, cancellationToken);

        if (AplicarAlteracoes(contexto, produtos, cupons))
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool AplicarAlteracoes(
        DbContext contexto, Dictionary<int, Produto> produtosNoBanco, Dictionary<int, Cupom> cuponsNoBanco)
    {
        foreach (var produto in CatalogoSeed.Produtos())
        {
            if (produtosNoBanco.TryGetValue(produto.Id, out var existente))
            {
                contexto.Entry(existente).CurrentValues.SetValues(produto);
            }
            else
            {
                contexto.Add(produto);
            }
        }

        foreach (var cupom in CatalogoSeed.Cupons())
        {
            if (cuponsNoBanco.TryGetValue(cupom.Id, out var existente))
            {
                contexto.Entry(existente).CurrentValues.SetValues(cupom);
            }
            else
            {
                contexto.Add(cupom);
            }
        }

        return contexto.ChangeTracker.HasChanges();
    }
}

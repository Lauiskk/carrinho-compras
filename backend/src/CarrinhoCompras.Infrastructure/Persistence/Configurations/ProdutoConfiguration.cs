using CarrinhoCompras.Domain.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarrinhoCompras.Infrastructure.Persistence.Configurations;

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public const string PropriedadeVersao = "Versao";

    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produto", tabela =>
        {
            tabela.HasCheckConstraint("CK_Produto_PrecoLiquido", "\"PrecoLiquido\" >= 0");
            tabela.HasCheckConstraint("CK_Produto_QuantidadeEstoque", "\"QuantidadeEstoque\" >= 0");

            // O banco também garante que ninguém reserve mais do que existe.
            tabela.HasCheckConstraint(
                "CK_Produto_QuantidadeReservada",
                "\"QuantidadeReservada\" >= 0 AND \"QuantidadeReservada\" <= \"QuantidadeEstoque\"");
        });

        builder.HasKey(produto => produto.Id);

        // O ID vem do catálogo (produtos.json); o banco não gera.
        builder.Property(produto => produto.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(produto => produto.DescricaoProduto)
            .HasMaxLength(Produto.DescricaoTamanhoMaximo)
            .IsRequired();

        builder.Property(produto => produto.PrecoLiquido)
            .HasPrecision(18, 2);

        builder.Property(produto => produto.QuantidadeEstoque);

        builder.Property(produto => produto.QuantidadeReservada);

        // Calculada a partir das duas colunas acima: não é persistida.
        builder.Ignore(produto => produto.QuantidadeDisponivel);

        // Concorrência otimista também no produto: sem isso, duas sacolas disputando a mesma última unidade
        // leriam "0 reservado" e as duas gravariam "1", vendendo a mesma peça duas vezes. Mapeia para xmin.
        builder.Property<uint>(PropriedadeVersao)
            .IsRowVersion();
    }
}

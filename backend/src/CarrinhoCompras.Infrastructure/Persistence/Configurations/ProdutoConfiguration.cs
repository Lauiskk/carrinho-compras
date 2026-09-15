using CarrinhoCompras.Domain.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarrinhoCompras.Infrastructure.Persistence.Configurations;

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produto", tabela =>
        {
            tabela.HasCheckConstraint("CK_Produto_PrecoLiquido", "\"PrecoLiquido\" >= 0");
            tabela.HasCheckConstraint("CK_Produto_QuantidadeEstoque", "\"QuantidadeEstoque\" >= 0");
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
    }
}

using CarrinhoCompras.Domain.Carrinhos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarrinhoCompras.Infrastructure.Persistence.Configurations;

internal sealed class ItemCarrinhoConfiguration : IEntityTypeConfiguration<ItemCarrinho>
{
    public const string ChavePrimaria = "PK_ItemCarrinho";

    public void Configure(EntityTypeBuilder<ItemCarrinho> builder)
    {
        builder.ToTable("ItemCarrinho", tabela =>
        {
            tabela.HasCheckConstraint("CK_ItemCarrinho_Quantidade", "\"Quantidade\" > 0");
            tabela.HasCheckConstraint("CK_ItemCarrinho_PrecoItem", "\"PrecoItem\" = \"PrecoUnitario\" * \"Quantidade\"");
        });

        // Chave composta: o banco também garante uma única linha por produto em cada carrinho.
        builder.HasKey(item => new { item.CarrinhoId, item.ProdutoId })
            .HasName(ChavePrimaria);

        builder.Property(item => item.CarrinhoId).HasColumnName("CarrinhoID");
        builder.Property(item => item.ProdutoId).HasColumnName("ProdutoID");

        builder.Property(item => item.PrecoUnitario).HasPrecision(18, 2);
        builder.Property(item => item.PrecoItem).HasPrecision(18, 2);

        builder.HasOne(item => item.Produto)
            .WithMany()
            .HasForeignKey(item => item.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using CarrinhoCompras.Domain.Cupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarrinhoCompras.Infrastructure.Persistence.Configurations;

internal sealed class CupomConfiguration : IEntityTypeConfiguration<Cupom>
{
    public void Configure(EntityTypeBuilder<Cupom> builder)
    {
        builder.ToTable("Cupom", tabela => tabela.HasCheckConstraint(
            "CK_Cupom_PercentualDesconto",
            "\"PercentualDesconto\" > 0 AND \"PercentualDesconto\" <= 100"));

        builder.HasKey(cupom => cupom.Id);

        // O ID vem do arquivo cupons.json; o banco não gera.
        builder.Property(cupom => cupom.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(cupom => cupom.CodigoCupom)
            .HasMaxLength(Cupom.CodigoTamanhoMaximo)
            .IsRequired();

        builder.HasIndex(cupom => cupom.CodigoCupom)
            .IsUnique();

        builder.Property(cupom => cupom.PercentualDesconto)
            .HasPrecision(5, 2);
    }
}

using CarrinhoCompras.Domain.Carrinhos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarrinhoCompras.Infrastructure.Persistence.Configurations;

internal sealed class CarrinhoConfiguration : IEntityTypeConfiguration<Carrinho>
{
    public const string PropriedadeVersao = "Versao";

    public void Configure(EntityTypeBuilder<Carrinho> builder)
    {
        builder.ToTable("Carrinho", tabela =>
        {
            tabela.HasCheckConstraint("CK_Carrinho_Status", "\"Status\" IN ('Aberto', 'Finalizado', 'Expirado')");
            tabela.HasCheckConstraint(
                "CK_Carrinho_Valores",
                "\"Subtotal\" >= 0 AND \"Desconto\" >= 0 AND \"Desconto\" <= \"Subtotal\" AND \"Total\" = \"Subtotal\" - \"Desconto\"");
        });

        builder.HasKey(carrinho => carrinho.Id);

        // Guid v7 gerado pelo domínio.
        builder.Property(carrinho => carrinho.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        // Gravado como texto ("Aberto"/"Finalizado"/"Expirado"): legível em consultas e estável se o enum mudar de ordem.
        builder.Property(carrinho => carrinho.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(carrinho => carrinho.CupomId)
            .HasColumnName("CupomID");

        builder.Property(carrinho => carrinho.Subtotal).HasPrecision(18, 2);
        builder.Property(carrinho => carrinho.Desconto).HasPrecision(18, 2);
        builder.Property(carrinho => carrinho.Total).HasPrecision(18, 2);

        // Só a sacola aberta com itens tem prazo; as demais têm ExpiraEm nulo.
        builder.Property(carrinho => carrinho.ExpiraEm);

        builder.Ignore(carrinho => carrinho.EstaFinalizado);
        builder.Ignore(carrinho => carrinho.EstaExpirado);

        builder.HasOne(carrinho => carrinho.Cupom)
            .WithMany()
            .HasForeignKey(carrinho => carrinho.CupomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(carrinho => carrinho.Itens)
            .WithOne()
            .HasForeignKey(item => item.CarrinhoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(carrinho => carrinho.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Concorrência otimista: uma propriedade uint marcada como row version é mapeada pelo Npgsql para a
        // coluna de sistema xmin do PostgreSQL, que muda a cada UPDATE da linha. Fica como shadow property
        // para não levar detalhes de persistência ao domínio.
        builder.Property<uint>(PropriedadeVersao)
            .IsRowVersion();
    }
}

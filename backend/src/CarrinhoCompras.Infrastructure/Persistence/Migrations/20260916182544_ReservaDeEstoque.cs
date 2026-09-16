using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarrinhoCompras.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReservaDeEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Carrinho_Status",
                table: "Carrinho");

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeReservada",
                table: "Produto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Produto",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiraEm",
                table: "Carrinho",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Produto_QuantidadeReservada",
                table: "Produto",
                sql: "\"QuantidadeReservada\" >= 0 AND \"QuantidadeReservada\" <= \"QuantidadeEstoque\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Carrinho_Status",
                table: "Carrinho",
                sql: "\"Status\" IN ('Aberto', 'Finalizado', 'Expirado')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Produto_QuantidadeReservada",
                table: "Produto");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Carrinho_Status",
                table: "Carrinho");

            migrationBuilder.DropColumn(
                name: "QuantidadeReservada",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "ExpiraEm",
                table: "Carrinho");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Carrinho_Status",
                table: "Carrinho",
                sql: "\"Status\" IN ('Aberto', 'Finalizado')");
        }
    }
}

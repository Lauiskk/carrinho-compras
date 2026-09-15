using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarrinhoCompras.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EstruturaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cupom",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    CodigoCupom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupom", x => x.ID);
                    table.CheckConstraint("CK_Cupom_PercentualDesconto", "\"PercentualDesconto\" > 0 AND \"PercentualDesconto\" <= 100");
                });

            migrationBuilder.CreateTable(
                name: "Produto",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    DescricaoProduto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrecoLiquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantidadeEstoque = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produto", x => x.ID);
                    table.CheckConstraint("CK_Produto_PrecoLiquido", "\"PrecoLiquido\" >= 0");
                    table.CheckConstraint("CK_Produto_QuantidadeEstoque", "\"QuantidadeEstoque\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Carrinho",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CupomID = table.Column<int>(type: "integer", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinalizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carrinho", x => x.ID);
                    table.CheckConstraint("CK_Carrinho_Status", "\"Status\" IN ('Aberto', 'Finalizado')");
                    table.CheckConstraint("CK_Carrinho_Valores", "\"Subtotal\" >= 0 AND \"Desconto\" >= 0 AND \"Desconto\" <= \"Subtotal\" AND \"Total\" = \"Subtotal\" - \"Desconto\"");
                    table.ForeignKey(
                        name: "FK_Carrinho_Cupom_CupomID",
                        column: x => x.CupomID,
                        principalTable: "Cupom",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemCarrinho",
                columns: table => new
                {
                    CarrinhoID = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoID = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecoItem = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCarrinho", x => new { x.CarrinhoID, x.ProdutoID });
                    table.CheckConstraint("CK_ItemCarrinho_PrecoItem", "\"PrecoItem\" = \"PrecoUnitario\" * \"Quantidade\"");
                    table.CheckConstraint("CK_ItemCarrinho_Quantidade", "\"Quantidade\" > 0");
                    table.ForeignKey(
                        name: "FK_ItemCarrinho_Carrinho_CarrinhoID",
                        column: x => x.CarrinhoID,
                        principalTable: "Carrinho",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemCarrinho_Produto_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produto",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carrinho_CupomID",
                table: "Carrinho",
                column: "CupomID");

            migrationBuilder.CreateIndex(
                name: "IX_Cupom_CodigoCupom",
                table: "Cupom",
                column: "CodigoCupom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCarrinho_ProdutoID",
                table: "ItemCarrinho",
                column: "ProdutoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemCarrinho");

            migrationBuilder.DropTable(
                name: "Carrinho");

            migrationBuilder.DropTable(
                name: "Produto");

            migrationBuilder.DropTable(
                name: "Cupom");
        }
    }
}

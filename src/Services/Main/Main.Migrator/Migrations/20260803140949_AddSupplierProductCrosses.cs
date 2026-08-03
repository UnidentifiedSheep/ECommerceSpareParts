using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierProductCrosses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplier_product_crosses",
                schema: "catalogue_enrichment",
                columns: table => new
                {
                    left_supplier_product_id = table.Column<int>(type: "integer", nullable: false),
                    right_supplier_product_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supplier_product_crosses_pk", x => new { x.left_supplier_product_id, x.right_supplier_product_id });
                    table.ForeignKey(
                        name: "supplier_product_crosses_left_product_id_fk",
                        column: x => x.left_supplier_product_id,
                        principalSchema: "catalogue_enrichment",
                        principalTable: "supplier_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "supplier_product_crosses_right_product_id_fk",
                        column: x => x.right_supplier_product_id,
                        principalSchema: "catalogue_enrichment",
                        principalTable: "supplier_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "supplier_product_crosses_right_product_id_idx",
                schema: "catalogue_enrichment",
                table: "supplier_product_crosses",
                column: "right_supplier_product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_product_crosses",
                schema: "catalogue_enrichment");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductPairIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_products_PairId",
                schema: "public",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "PairId",
                schema: "public",
                table: "products",
                newName: "pair_id");

            migrationBuilder.AddForeignKey(
                name: "FK_products_products_pair_id",
                schema: "public",
                table: "products",
                column: "pair_id",
                principalSchema: "public",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_products_pair_id",
                schema: "public",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "pair_id",
                schema: "public",
                table: "products",
                newName: "PairId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_products_PairId",
                schema: "public",
                table: "products",
                column: "PairId",
                principalSchema: "public",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

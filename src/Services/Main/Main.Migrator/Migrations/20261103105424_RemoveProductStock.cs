using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "products_stock_index",
                schema: "public",
                table: "products");

            migrationBuilder.DropColumn(
                name: "stock",
                schema: "public",
                table: "products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "stock",
                schema: "public",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE public.products AS product
                SET stock = COALESCE((
                    SELECT SUM(balance.quantity)::integer
                    FROM public.product_stocks AS balance
                    WHERE balance.product_id = product.id
                ), 0);
                """);

            migrationBuilder.CreateIndex(
                name: "products_stock_index",
                schema: "public",
                table: "products",
                column: "stock")
                .Annotation("Relational:ColumnName", "stock");
        }
    }
}

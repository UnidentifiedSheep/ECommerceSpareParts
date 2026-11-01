using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class SaleContentAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "price",
                schema: "public",
                table: "sale_content",
                newName: "discounted_price");

            migrationBuilder.RenameColumn(
                name: "discount",
                schema: "public",
                table: "sale_content",
                newName: "original_price");

            // Existing price was discounted; discount stored a fraction, not a monetary amount.
            migrationBuilder.Sql("UPDATE public.sale_content SET original_price = round(discounted_price / (1 - original_price), 2)");

            migrationBuilder.CreateTable(
                name: "sale_content_analytics",
                schema: "public",
                columns: table => new
                {
                    sale_content_id = table.Column<int>(type: "integer", nullable: false),
                    original_amount_in_base_currency = table.Column<decimal>(type: "numeric", nullable: false),
                    revenue_in_base_currency = table.Column<decimal>(type: "numeric", nullable: false),
                    cost_in_base_currency = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_content_analytics_pk", x => x.sale_content_id);
                    table.ForeignKey(
                        name: "sale_content_analytics_sale_content_id_fk",
                        column: x => x.sale_content_id,
                        principalSchema: "public",
                        principalTable: "sale_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sale_content_analytics",
                schema: "public");

            migrationBuilder.Sql("UPDATE public.sale_content SET original_price = (original_price - discounted_price) / original_price");

            migrationBuilder.RenameColumn(
                name: "original_price",
                schema: "public",
                table: "sale_content",
                newName: "discount");

            migrationBuilder.RenameColumn(
                name: "discounted_price",
                schema: "public",
                table: "sale_content",
                newName: "price");
        }
    }
}

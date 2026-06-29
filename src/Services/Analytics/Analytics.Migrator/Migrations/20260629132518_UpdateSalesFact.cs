using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSalesFact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "base_currency_id",
                table: "sales_fact",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "price_in_base_currency",
                table: "sale_contents",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "buy_price",
                table: "sale_content_detail",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "buy_price_in_base_currency",
                table: "sale_content_detail",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "sale_content_id",
                table: "sale_content_detail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "sale_content_detail_sale_content_id_index",
                table: "sale_content_detail",
                column: "sale_content_id");

            migrationBuilder.AddForeignKey(
                name: "sale_content_detail_sale_content_id_fk",
                table: "sale_content_detail",
                column: "sale_content_id",
                principalTable: "sale_contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "sale_content_detail_sale_content_id_fk",
                table: "sale_content_detail");

            migrationBuilder.DropIndex(
                name: "sale_content_detail_sale_content_id_index",
                table: "sale_content_detail");

            migrationBuilder.DropColumn(
                name: "base_currency_id",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "price_in_base_currency",
                table: "sale_contents");

            migrationBuilder.DropColumn(
                name: "buy_price_in_base_currency",
                table: "sale_content_detail");

            migrationBuilder.DropColumn(
                name: "sale_content_id",
                table: "sale_content_detail");

            migrationBuilder.AlterColumn<decimal>(
                name: "buy_price",
                table: "sale_content_detail",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}

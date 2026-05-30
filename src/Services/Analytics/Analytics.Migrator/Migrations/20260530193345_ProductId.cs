using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "article_id",
                table: "sale_contents",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "sale_contents_article_id_index",
                table: "sale_contents",
                newName: "sale_contents_product_id_index");

            migrationBuilder.RenameColumn(
                name: "article_id",
                table: "purchase_contents",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "purchase_contents_article_id_index",
                table: "purchase_contents",
                newName: "purchase_contents_product_id_index");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "sale_contents",
                newName: "article_id");

            migrationBuilder.RenameIndex(
                name: "sale_contents_product_id_index",
                table: "sale_contents",
                newName: "sale_contents_article_id_index");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "purchase_contents",
                newName: "article_id");

            migrationBuilder.RenameIndex(
                name: "purchase_contents_product_id_index",
                table: "purchase_contents",
                newName: "purchase_contents_article_id_index");
        }
    }
}

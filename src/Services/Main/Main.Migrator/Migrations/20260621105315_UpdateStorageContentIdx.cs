using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStorageContentIdx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "storage_content_buy_price_index",
                schema: "public",
                table: "storage_content");

            migrationBuilder.DropIndex(
                name: "storage_content_product_id_count_index",
                schema: "public",
                table: "storage_content");

            migrationBuilder.DropIndex(
                name: "storage_content_purchase_datetime_index",
                schema: "public",
                table: "storage_content");

            migrationBuilder.DropIndex(
                name: "storage_content_storage_name_index",
                schema: "public",
                table: "storage_content");

            migrationBuilder.DropIndex(
                name: "products_sku_index",
                schema: "public",
                table: "products");

            migrationBuilder.DropIndex(
                name: "products_stock_index",
                schema: "public",
                table: "products");

            migrationBuilder.DropIndex(
                name: "users_normalized_user_name_index",
                schema: "auth",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "products_sku_index",
                schema: "public",
                table: "products",
                column: "normalized_sku")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" })
                .Annotation("Relational:ColumnName", "normalized_sku");

            migrationBuilder.CreateIndex(
                name: "products_stock_index",
                schema: "public",
                table: "products",
                column: "stock")
                .Annotation("Relational:ColumnName", "stock");

            migrationBuilder.CreateIndex(
                name: "users_normalized_user_name_index",
                schema: "auth",
                table: "users",
                column: "normalized_user_name")
                .Annotation("MaxLength", 36)
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" })
                .Annotation("Relational:ColumnName", "normalized_user_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "storage_content_buy_price_index",
                schema: "public",
                table: "storage_content",
                column: "buy_price");

            migrationBuilder.CreateIndex(
                name: "storage_content_product_id_count_index",
                schema: "public",
                table: "storage_content",
                columns: new[] { "product_id", "count" });

            migrationBuilder.CreateIndex(
                name: "storage_content_purchase_datetime_index",
                schema: "public",
                table: "storage_content",
                column: "purchase_datetime");

            migrationBuilder.CreateIndex(
                name: "storage_content_storage_name_index",
                schema: "public",
                table: "storage_content",
                column: "storage_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.DropIndex(
                name: "products_sku_index",
                schema: "public",
                table: "products");

            migrationBuilder.DropIndex(
                name: "products_stock_index",
                schema: "public",
                table: "products");

            migrationBuilder.DropIndex(
                name: "users_normalized_user_name_index",
                schema: "auth",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "products_sku_index",
                schema: "public",
                table: "products",
                column: "normalized_sku")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" })
                .Annotation("Relational:ColumnName", "normalized_sku")
                .Annotation("Relational:ColumnType", "text");

            migrationBuilder.CreateIndex(
                name: "products_stock_index",
                schema: "public",
                table: "products",
                column: "stock")
                .Annotation("Relational:ColumnName", "stock")
                .Annotation("Relational:ColumnType", "integer");

            migrationBuilder.CreateIndex(
                name: "users_normalized_user_name_index",
                schema: "auth",
                table: "users",
                column: "normalized_user_name")
                .Annotation("MaxLength", 36)
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" })
                .Annotation("Relational:ColumnName", "normalized_user_name")
                .Annotation("Relational:ColumnType", "character varying(36)");
        }
    }
}

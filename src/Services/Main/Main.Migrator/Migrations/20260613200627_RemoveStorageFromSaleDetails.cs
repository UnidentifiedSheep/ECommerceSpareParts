using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStorageFromSaleDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "sale_content_details_storage_content_id_fk",
                schema: "public",
                table: "sale_content_details");

            migrationBuilder.DropForeignKey(
                name: "sale_content_details_storages_name_fk",
                schema: "public",
                table: "sale_content_details");

            migrationBuilder.DropIndex(
                name: "sale_content_details_storage_index",
                schema: "public",
                table: "sale_content_details");

            migrationBuilder.DropColumn(
                name: "storage",
                schema: "public",
                table: "sale_content_details");

            migrationBuilder.AddForeignKey(
                name: "sale_content_details_storage_content_id_fk",
                schema: "public",
                table: "sale_content_details",
                column: "storage_content_id",
                principalSchema: "public",
                principalTable: "storage_content",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

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
            migrationBuilder.DropForeignKey(
                name: "sale_content_details_storage_content_id_fk",
                schema: "public",
                table: "sale_content_details");

            migrationBuilder.AddColumn<string>(
                name: "storage",
                schema: "public",
                table: "sale_content_details",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_storage_index",
                schema: "public",
                table: "sale_content_details",
                column: "storage");

            migrationBuilder.AddForeignKey(
                name: "sale_content_details_storage_content_id_fk",
                schema: "public",
                table: "sale_content_details",
                column: "storage_content_id",
                principalSchema: "public",
                principalTable: "storage_content",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "sale_content_details_storages_name_fk",
                schema: "public",
                table: "sale_content_details",
                column: "storage",
                principalSchema: "public",
                principalTable: "storages",
                principalColumn: "name",
                onDelete: ReferentialAction.Restrict);

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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class TransactionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "transactions_receiver_id_transaction_datetime_id_idx",
                schema: "public",
                table: "transactions",
                columns: new[] { "receiver_id", "transaction_datetime", "id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "transactions_sender_id_transaction_datetime_id_idx",
                schema: "public",
                table: "transactions",
                columns: new[] { "sender_id", "transaction_datetime", "id" },
                descending: new[] { false, true, true });

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
            migrationBuilder.DropIndex(
                name: "transactions_receiver_id_transaction_datetime_id_idx",
                schema: "public",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "transactions_sender_id_transaction_datetime_id_idx",
                schema: "public",
                table: "transactions");

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

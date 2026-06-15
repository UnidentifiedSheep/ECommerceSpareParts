using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ProductReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "storage_content_reservations_is_done_index",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropIndex(
                name: "storage_content_reservations_product_id_is_done_index",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropIndex(
                name: "storage_content_reservations_product_id_is_locked_index",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropIndex(
                name: "storage_content_reservations_user_id_is_done_index",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropColumn(
                name: "is_done",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropColumn(
                name: "is_locked",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "public",
                table: "storage_content_reservations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                schema: "public",
                table: "events",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);

            migrationBuilder.AddColumn<int>(
                name: "reservation_id",
                schema: "public",
                table: "events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_product_id_status_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "product_id", "status" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_status_index",
                schema: "public",
                table: "storage_content_reservations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_user_id_status_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "reservation_manual_change_event_reservation_id_idx",
                schema: "public",
                table: "events",
                column: "reservation_id");

            migrationBuilder.AddForeignKey(
                name: "reservation_manual_change_event_reservation_id_fk",
                schema: "public",
                table: "events",
                column: "reservation_id",
                principalSchema: "public",
                principalTable: "storage_content_reservations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
                name: "reservation_manual_change_event_reservation_id_fk",
                schema: "public",
                table: "events");

            migrationBuilder.DropIndex(
                name: "storage_content_reservations_product_id_status_index",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropIndex(
                name: "storage_content_reservations_status_index",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropIndex(
                name: "storage_content_reservations_user_id_status_index",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropIndex(
                name: "reservation_manual_change_event_reservation_id_idx",
                schema: "public",
                table: "events");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropColumn(
                name: "reservation_id",
                schema: "public",
                table: "events");

            migrationBuilder.AddColumn<bool>(
                name: "is_done",
                schema: "public",
                table: "storage_content_reservations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_locked",
                schema: "public",
                table: "storage_content_reservations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                schema: "public",
                table: "events",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34);

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_is_done_index",
                schema: "public",
                table: "storage_content_reservations",
                column: "is_done");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_product_id_is_done_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "product_id", "is_done" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_product_id_is_locked_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "product_id", "is_locked" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_user_id_is_done_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "user_id", "is_done" });

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

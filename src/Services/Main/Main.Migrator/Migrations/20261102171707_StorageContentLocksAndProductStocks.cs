using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class StorageContentLocksAndProductStocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_stocks",
                schema: "public",
                columns: table => new
                {
                    storage_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_stocks_pk", x => new { x.storage_code, x.product_id });
                    table.CheckConstraint("product_stocks_quantity_nonnegative_ck", "quantity >= 0");
                    table.ForeignKey(
                        name: "product_stocks_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "product_stocks_storage_code_fk",
                        column: x => x.storage_code,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_content_locks",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_content_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_content_locks_pk", x => x.id);
                    table.CheckConstraint("storage_content_locks_quantity_positive_ck", "quantity > 0");
                    table.ForeignKey(
                        name: "storage_content_locks_storage_content_id_fk",
                        column: x => x.storage_content_id,
                        principalSchema: "public",
                        principalTable: "storage_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "product_stocks_product_id_idx",
                schema: "public",
                table: "product_stocks",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storagecontentlock_who_created_idx",
                schema: "public",
                table: "storage_content_locks",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storagecontentlock_who_updated_idx",
                schema: "public",
                table: "storage_content_locks",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_content_locks_active_content_idx",
                schema: "public",
                table: "storage_content_locks",
                column: "storage_content_id",
                filter: "state = 'Active'")
                .Annotation("Npgsql:IndexInclude", new[] { "quantity", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "storage_content_locks_active_expiration_idx",
                schema: "public",
                table: "storage_content_locks",
                columns: new[] { "expires_at", "id" },
                filter: "state = 'Active' AND expires_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "storage_content_locks_storage_content_id_idx",
                schema: "public",
                table: "storage_content_locks",
                column: "storage_content_id");

            migrationBuilder.Sql("""
                INSERT INTO public.product_stocks (storage_code, product_id, quantity)
                SELECT storage_name, product_id, SUM(count)::integer
                FROM public.storage_content
                GROUP BY storage_name, product_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_stocks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage_content_locks",
                schema: "public");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class OrderFulfillments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_fulfillments",
                schema: "public",
                columns: table => new
                {
                    order_item_id = table.Column<int>(type: "integer", nullable: false),
                    sale_content_id = table.Column<int>(type: "integer", nullable: false),
                    covered_quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_fulfillments_pk", x => new { x.order_item_id, x.sale_content_id });
                    table.ForeignKey(
                        name: "order_fulfillments_order_item_id_fk",
                        column: x => x.order_item_id,
                        principalSchema: "public",
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_fulfillments_sale_content_id_fk",
                        column: x => x.sale_content_id,
                        principalSchema: "public",
                        principalTable: "sale_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "jobs_created_at_idx",
                schema: "job",
                table: "jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "order_fulfillments_sale_content_id_index",
                schema: "public",
                table: "order_fulfillments",
                column: "sale_content_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_fulfillments",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "jobs_created_at_idx",
                schema: "job",
                table: "jobs");
        }
    }
}

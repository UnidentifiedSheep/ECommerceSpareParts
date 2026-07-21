using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class PriceAppliers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_appliers",
                schema: "public",
                columns: table => new
                {
                    system_name = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    dsl_logic = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_appliers_pk", x => x.system_name);
                });

            migrationBuilder.CreateTable(
                name: "price_applier_states",
                schema: "public",
                columns: table => new
                {
                    price_applier_system_name = table.Column<string>(type: "text", nullable: false),
                    usage = table.Column<string>(type: "text", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_applier_states_pk", x => new { x.price_applier_system_name, x.usage });
                    table.ForeignKey(
                        name: "price_applier_states_price_applier_system_name_fk",
                        column: x => x.price_applier_system_name,
                        principalSchema: "public",
                        principalTable: "price_appliers",
                        principalColumn: "system_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "price_applier_states_enabled_usage_order_uq",
                schema: "public",
                table: "price_applier_states",
                columns: new[] { "usage", "order" },
                unique: true,
                filter: "enabled = true");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplierstate_who_created_idx",
                schema: "public",
                table: "price_applier_states",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplierstate_who_updated_idx",
                schema: "public",
                table: "price_applier_states",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplier_who_created_idx",
                schema: "public",
                table: "price_appliers",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplier_who_updated_idx",
                schema: "public",
                table: "price_appliers",
                column: "who_updated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_applier_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_appliers",
                schema: "public");
        }
    }
}

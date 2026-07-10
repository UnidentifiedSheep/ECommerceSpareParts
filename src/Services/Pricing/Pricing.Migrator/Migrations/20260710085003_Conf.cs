using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Conf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_offer_refresh_states",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    last_offers_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_offers_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_offer_refresh_state_pk", x => new { x.product_id, x.source, x.storage_name });
                });

            migrationBuilder.CreateTable(
                name: "price_recalculation_requests",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_recalculation_request_pk", x => new { x.product_id, x.storage_name });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_offer_refresh_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_recalculation_requests",
                schema: "public");
        }
    }
}

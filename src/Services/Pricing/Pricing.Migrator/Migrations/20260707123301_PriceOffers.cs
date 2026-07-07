using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class PriceOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_offers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    offer_for_storage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    available_quantity = table.Column<int>(type: "integer", nullable: false),
                    minimum_purchase_quantity = table.Column<int>(type: "integer", nullable: false),
                    quantity_coefficient = table.Column<int>(type: "integer", nullable: false),
                    days_to_refund = table.Column<int>(type: "integer", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guaranteed_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivery_probability = table.Column<int>(type: "integer", nullable: false),
                    order_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_offer_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "price_offer_currency_id_index",
                table: "price_offers",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "price_offer_expires_at_index",
                table: "price_offers",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "price_offer_offer_for_storage_index",
                table: "price_offers",
                column: "offer_for_storage");

            migrationBuilder.CreateIndex(
                name: "price_offer_product_id_index",
                table: "price_offers",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "price_offer_source_source_key_index",
                table: "price_offers",
                columns: new[] { "source", "source_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "pricing.entities.priceoffer_who_created_idx",
                table: "price_offers",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.priceoffer_who_updated_idx",
                table: "price_offers",
                column: "who_updated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_offers");
        }
    }
}

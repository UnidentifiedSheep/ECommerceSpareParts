using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdUniqIdx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "price_offer_product_id_index",
                table: "price_offers");

            migrationBuilder.DropIndex(
                name: "price_offer_source_source_key_index",
                table: "price_offers");

            migrationBuilder.CreateIndex(
                name: "price_offer_product_id_index",
                table: "price_offers",
                columns: new[] { "product_id", "offer_for_storage" });

            migrationBuilder.CreateIndex(
                name: "price_offer_source_source_key_index",
                table: "price_offers",
                columns: new[] { "source", "source_key", "offer_for_storage" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "price_offer_product_id_index",
                table: "price_offers");

            migrationBuilder.DropIndex(
                name: "price_offer_source_source_key_index",
                table: "price_offers");

            migrationBuilder.CreateIndex(
                name: "price_offer_product_id_index",
                table: "price_offers",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "price_offer_source_source_key_index",
                table: "price_offers",
                columns: new[] { "source", "source_key" },
                unique: true);
        }
    }
}

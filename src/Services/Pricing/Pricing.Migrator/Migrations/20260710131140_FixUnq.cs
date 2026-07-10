using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class FixUnq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "price_offer_source_source_key_uq",
                schema: "public",
                table: "price_offers");

            migrationBuilder.AddUniqueConstraint(
                name: "price_offer_source_source_key_uq",
                schema: "public",
                table: "price_offers",
                columns: new[] { "product_id", "source", "source_key", "offer_for_storage" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "price_offer_source_source_key_uq",
                schema: "public",
                table: "price_offers");

            migrationBuilder.AddUniqueConstraint(
                name: "price_offer_source_source_key_uq",
                schema: "public",
                table: "price_offers",
                columns: new[] { "source", "source_key", "offer_for_storage" });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ProductPriceOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_price_options",
                schema: "public",
                columns: table => new
                {
                    price_offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    markup = table.Column<decimal>(type: "numeric", nullable: false),
                    for_storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    delivery_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    guaranteed_delivery_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    delivery_probability = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_price_option_pk", x => x.price_offer_id);
                    table.ForeignKey(
                        name: "product_price_options_price_offer_id_fk",
                        column: x => x.price_offer_id,
                        principalSchema: "public",
                        principalTable: "price_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "pricing.entities.productpriceoption_who_created_idx",
                schema: "public",
                table: "product_price_options",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.productpriceoption_who_updated_idx",
                schema: "public",
                table: "product_price_options",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "product_price_option_currency_id_index",
                schema: "public",
                table: "product_price_options",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "product_price_option_score_index",
                schema: "public",
                table: "product_price_options",
                column: "score");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_price_options",
                schema: "public");
        }
    }
}

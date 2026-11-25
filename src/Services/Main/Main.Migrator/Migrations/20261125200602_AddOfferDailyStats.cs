using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferDailyStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "delivery_daily_stats",
                schema: "logistics",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    supplier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    storage_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    delivery_days = table.Column<int>(type: "integer", nullable: false),
                    offers_count = table.Column<int>(type: "integer", nullable: false),
                    max_probability = table.Column<int>(type: "integer", nullable: false),
                    min_guaranteed_days = table.Column<int>(type: "integer", nullable: true),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("delivery_daily_stat_pk", x => new { x.product_id, x.supplier, x.storage_code, x.day, x.delivery_days });
                    table.ForeignKey(
                        name: "delivery_daily_stat_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "delivery_daily_stat_storage_code_fk",
                        column: x => x.storage_code,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "price_offer_daily_stats",
                schema: "public",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    for_storage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    offers_count = table.Column<int>(type: "integer", nullable: false),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_offer_daily_stat_pk", x => new { x.product_id, x.source, x.for_storage, x.day, x.currency_id, x.price });
                    table.ForeignKey(
                        name: "price_offer_daily_stat_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "price_offer_daily_stat_for_storage_fk",
                        column: x => x.for_storage,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "price_offer_daily_stat_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "delivery_daily_stat_storage_code_index",
                schema: "logistics",
                table: "delivery_daily_stats",
                column: "storage_code");

            migrationBuilder.CreateIndex(
                name: "price_offer_daily_stat_currency_id_index",
                schema: "public",
                table: "price_offer_daily_stats",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "price_offer_daily_stat_for_storage_index",
                schema: "public",
                table: "price_offer_daily_stats",
                column: "for_storage");

            migrationBuilder.Sql(
                """
                DELETE FROM public.price_offers o
                WHERE NOT EXISTS (SELECT 1 FROM public.products p WHERE p.id = o.product_id)
                   OR NOT EXISTS (SELECT 1 FROM public.currency c WHERE c.id = o.currency_id)
                   OR NOT EXISTS (SELECT 1 FROM public.storages s WHERE s.code = o.offer_for_storage);
                """);

            migrationBuilder.AddForeignKey(
                name: "price_offer_currency_id_fk",
                schema: "public",
                table: "price_offers",
                column: "currency_id",
                principalSchema: "public",
                principalTable: "currency",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "price_offer_offer_for_storage_fk",
                schema: "public",
                table: "price_offers",
                column: "offer_for_storage",
                principalSchema: "public",
                principalTable: "storages",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "price_offer_product_id_fk",
                schema: "public",
                table: "price_offers",
                column: "product_id",
                principalSchema: "public",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "price_offer_currency_id_fk",
                schema: "public",
                table: "price_offers");

            migrationBuilder.DropForeignKey(
                name: "price_offer_offer_for_storage_fk",
                schema: "public",
                table: "price_offers");

            migrationBuilder.DropForeignKey(
                name: "price_offer_product_id_fk",
                schema: "public",
                table: "price_offers");

            migrationBuilder.DropTable(
                name: "delivery_daily_stats",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "price_offer_daily_stats",
                schema: "public");
        }
    }
}

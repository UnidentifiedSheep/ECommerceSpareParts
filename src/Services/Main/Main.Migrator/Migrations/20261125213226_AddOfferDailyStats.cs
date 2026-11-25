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
            migrationBuilder.AddColumn<int>(
                name: "delivery_days",
                schema: "public",
                table: "price_offers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "guaranteed_delivery_days",
                schema: "public",
                table: "price_offers",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE public.price_offers
                SET delivery_days = GREATEST(delivery_date::date - updated_at::date, 0),
                    guaranteed_delivery_days = GREATEST(guaranteed_delivery_date::date - updated_at::date, 0)
                WHERE delivery_date IS NOT NULL OR guaranteed_delivery_date IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "delivery_date",
                schema: "public",
                table: "price_offers");

            migrationBuilder.DropColumn(
                name: "guaranteed_delivery_date",
                schema: "public",
                table: "price_offers");

            migrationBuilder.AddColumn<int>(
                name: "delivery_days",
                schema: "public",
                table: "product_price_options",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "guaranteed_delivery_days",
                schema: "public",
                table: "product_price_options",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE public.product_price_options
                SET delivery_days = CEIL(EXTRACT(EPOCH FROM delivery_time) / 86400)::integer,
                    guaranteed_delivery_days = CEIL(EXTRACT(EPOCH FROM guaranteed_delivery_time) / 86400)::integer;
                """);

            migrationBuilder.DropColumn(
                name: "delivery_time",
                schema: "public",
                table: "product_price_options");

            migrationBuilder.DropColumn(
                name: "guaranteed_delivery_time",
                schema: "public",
                table: "product_price_options");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "arrival_time",
                schema: "logistics",
                table: "delivery_schedules",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(23, 59, 59));

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
            migrationBuilder.AddColumn<DateTime>(
                name: "delivery_date",
                schema: "public",
                table: "price_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "guaranteed_delivery_date",
                schema: "public",
                table: "price_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE public.price_offers
                SET delivery_date = updated_at::date + delivery_days,
                    guaranteed_delivery_date = updated_at::date + guaranteed_delivery_days
                WHERE delivery_days IS NOT NULL OR guaranteed_delivery_days IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "delivery_days",
                schema: "public",
                table: "price_offers");

            migrationBuilder.DropColumn(
                name: "guaranteed_delivery_days",
                schema: "public",
                table: "price_offers");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "delivery_time",
                schema: "public",
                table: "product_price_options",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "guaranteed_delivery_time",
                schema: "public",
                table: "product_price_options",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.Sql(
                """
                UPDATE public.product_price_options
                SET delivery_time = make_interval(days => delivery_days),
                    guaranteed_delivery_time = make_interval(days => guaranteed_delivery_days);
                """);

            migrationBuilder.DropColumn(
                name: "delivery_days",
                schema: "public",
                table: "product_price_options");

            migrationBuilder.DropColumn(
                name: "guaranteed_delivery_days",
                schema: "public",
                table: "product_price_options");

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

            migrationBuilder.DropColumn(
                name: "arrival_time",
                schema: "logistics",
                table: "delivery_schedules");
        }
    }
}

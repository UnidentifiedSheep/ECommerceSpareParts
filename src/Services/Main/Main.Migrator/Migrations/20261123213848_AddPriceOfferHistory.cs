using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceOfferHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_offer_histories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    for_storage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    delivery_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    delivery_probability = table.Column<int>(type: "integer", nullable: false),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_open = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_offer_history_pk", x => x.id);
                    table.ForeignKey(
                        name: "price_offer_history_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "price_offer_history_for_storage_fk",
                        column: x => x.for_storage,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "price_offer_history_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "price_offer_history_currency_id_index",
                schema: "public",
                table: "price_offer_histories",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "price_offer_history_for_storage_index",
                schema: "public",
                table: "price_offer_histories",
                column: "for_storage");

            migrationBuilder.CreateIndex(
                name: "price_offer_history_open_index",
                schema: "public",
                table: "price_offer_histories",
                columns: new[] { "product_id", "source", "for_storage" },
                filter: "is_open = true");

            migrationBuilder.CreateIndex(
                name: "price_offer_history_product_index",
                schema: "public",
                table: "price_offer_histories",
                columns: new[] { "product_id", "for_storage", "first_seen_at" });

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
                name: "price_offer_histories",
                schema: "public");
        }
    }
}

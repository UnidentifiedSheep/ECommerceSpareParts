using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class PricingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.tmtrsuppliersetting_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "pricing.entities.settings.pricingsetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.tmtrsuppliersetting_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "pricing.entities.settings.pricingsetting_who_created_idx");

            migrationBuilder.CreateTable(
                name: "markup_group",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    source_message_id = table.Column<Guid>(type: "uuid", nullable: true),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("markup_group_pk", x => x.id);
                });

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
                name: "price_offers",
                schema: "public",
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
                    delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    guaranteed_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    source_occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_probability = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_offer_pk", x => x.id);
                    table.UniqueConstraint("price_offer_source_source_key_uq", x => new { x.product_id, x.source, x.source_key, x.offer_for_storage });
                });

            migrationBuilder.CreateTable(
                name: "markup_ranges",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    range_start = table.Column<decimal>(type: "numeric", nullable: false),
                    range_end = table.Column<decimal>(type: "numeric", nullable: false),
                    markup = table.Column<decimal>(type: "numeric", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("markup_ranges_pk", x => x.id);
                    table.ForeignKey(
                        name: "markup_ranges_markup_group_id_fk",
                        column: x => x.group_id,
                        principalSchema: "public",
                        principalTable: "markup_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    markup_version = table.Column<string>(type: "text", nullable: false),
                    appliers_version = table.Column<string>(type: "text", nullable: false),
                    pricing_settings_version = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "IX_markup_group_currency_id",
                schema: "public",
                table: "markup_group",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "markup_group_source_message_id_uindex",
                schema: "public",
                table: "markup_group",
                column: "source_message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "pricing.entities.markupgroup_who_created_idx",
                schema: "public",
                table: "markup_group",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.markupgroup_who_updated_idx",
                schema: "public",
                table: "markup_group",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "IX_markup_ranges_group_id",
                schema: "public",
                table: "markup_ranges",
                column: "group_id");

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

            migrationBuilder.CreateIndex(
                name: "price_offer_currency_id_index",
                schema: "public",
                table: "price_offers",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "price_offer_expires_at_index",
                schema: "public",
                table: "price_offers",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "price_offer_offer_for_storage_index",
                schema: "public",
                table: "price_offers",
                column: "offer_for_storage");

            migrationBuilder.CreateIndex(
                name: "price_offer_product_id_index",
                schema: "public",
                table: "price_offers",
                columns: new[] { "product_id", "offer_for_storage" });

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.priceoffer_who_created_idx",
                schema: "public",
                table: "price_offers",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.priceoffer_who_updated_idx",
                schema: "public",
                table: "price_offers",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.productpriceoption_who_created_idx",
                schema: "public",
                table: "product_price_options",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.productpriceoption_who_updated_idx",
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
                name: "markup_ranges",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_applier_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_offer_refresh_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_price_options",
                schema: "public");

            migrationBuilder.DropTable(
                name: "markup_group",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_appliers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_offers",
                schema: "public");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.settings.pricingsetting_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.tmtrsuppliersetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.settings.pricingsetting_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.tmtrsuppliersetting_who_created_idx");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLogisticsAndStorageRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_content_logistics",
                schema: "public");

            migrationBuilder.DropTable(
                name: "purchase_logistics",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage_owners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage_routes",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_content_logistics",
                schema: "public",
                columns: table => new
                {
                    purchase_content_id = table.Column<int>(type: "integer", nullable: false),
                    area_m3 = table.Column<decimal>(type: "numeric", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_content_logistics_pk", x => x.purchase_content_id);
                    table.ForeignKey(
                        name: "purchase_content_logistics_purchase_content_id_fk",
                        column: x => x.purchase_content_id,
                        principalSchema: "public",
                        principalTable: "purchase_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_owners",
                schema: "public",
                columns: table => new
                {
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_owners_pk", x => new { x.storage_name, x.user_id });
                    table.ForeignKey(
                        name: "storage_owners_storages_name_fk",
                        column: x => x.storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_owners_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_routes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivery_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    distance_m = table.Column<int>(type: "integer", nullable: false),
                    from_storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    minimum_price = table.Column<decimal>(type: "numeric", nullable: false),
                    price_kg = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_m3 = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_order = table.Column<decimal>(type: "numeric", nullable: false),
                    pricing_model = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    route_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    to_storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_routes_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_routes_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_routes_storages_name_fk",
                        column: x => x.from_storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_routes_storages_name_fk_2",
                        column: x => x.to_storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_routes_users_id_fk",
                        column: x => x.carrier_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_logistics",
                schema: "public",
                columns: table => new
                {
                    purchase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    minimum_price = table.Column<decimal>(type: "numeric", nullable: true),
                    minimum_price_applied = table.Column<bool>(type: "boolean", nullable: false),
                    price_kg = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_m3 = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_order = table.Column<decimal>(type: "numeric", nullable: false),
                    pricing_model = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_logistics_pk", x => x.purchase_id);
                    table.ForeignKey(
                        name: "purchase_logistics_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_logistics_purchase_id_fk",
                        column: x => x.purchase_id,
                        principalSchema: "public",
                        principalTable: "purchase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "purchase_logistics_storage_routes_id_fk",
                        column: x => x.route_id,
                        principalSchema: "public",
                        principalTable: "storage_routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_logistics_transactions_id_fk",
                        column: x => x.transaction_id,
                        principalSchema: "public",
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_logistics_currency_id",
                schema: "public",
                table: "purchase_logistics",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_logistics_route_id",
                schema: "public",
                table: "purchase_logistics",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "purchase_logistics_transaction_id_uindex",
                schema: "public",
                table: "purchase_logistics",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageowner_who_created_idx",
                schema: "public",
                table: "storage_owners",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageowner_who_updated_idx",
                schema: "public",
                table: "storage_owners",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_owners_owner_id_index",
                schema: "public",
                table: "storage_owners",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_storage_routes_to_storage_name",
                schema: "public",
                table: "storage_routes",
                column: "to_storage_name");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageroute_who_created_idx",
                schema: "public",
                table: "storage_routes",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageroute_who_updated_idx",
                schema: "public",
                table: "storage_routes",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_from_to_active_uindex",
                schema: "public",
                table: "storage_routes",
                columns: new[] { "from_storage_name", "to_storage_name", "is_active" },
                unique: true,
                filter: "(is_active = true)");

            migrationBuilder.CreateIndex(
                name: "storage_routes_carrier_id_index",
                schema: "public",
                table: "storage_routes",
                column: "carrier_id");

            migrationBuilder.CreateIndex(
                name: "storage_routes_currency_id_index",
                schema: "public",
                table: "storage_routes",
                column: "currency_id");
        }
    }
}

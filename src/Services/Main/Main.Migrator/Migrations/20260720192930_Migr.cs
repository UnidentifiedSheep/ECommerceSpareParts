using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Migr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "product_enrichment");

            migrationBuilder.CreateTable(
                name: "supplier_products",
                schema: "product_enrichment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    producer = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    normalized_sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supplier_products_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_product_analogues",
                schema: "product_enrichment",
                columns: table => new
                {
                    supplier_product_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_analogue_product_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supplier_product_analogues_pk", x => new { x.supplier_product_id, x.supplier_analogue_product_id });
                    table.ForeignKey(
                        name: "supplier_product_analogues_supplier_analogue_product_id_fk",
                        column: x => x.supplier_analogue_product_id,
                        principalSchema: "product_enrichment",
                        principalTable: "supplier_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "supplier_product_analogues_supplier_product_id_fk",
                        column: x => x.supplier_product_id,
                        principalSchema: "product_enrichment",
                        principalTable: "supplier_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_product_mappings",
                schema: "product_enrichment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_product_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    last_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supplier_product_mappings_pk", x => x.id);
                    table.ForeignKey(
                        name: "supplier_product_mappings_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "supplier_product_mappings_supplier_product_id_fk",
                        column: x => x.supplier_product_id,
                        principalSchema: "product_enrichment",
                        principalTable: "supplier_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_product_names",
                schema: "product_enrichment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_product_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    supplier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supplier_product_names_pk", x => x.id);
                    table.ForeignKey(
                        name: "supplier_product_names_supplier_product_id_fk",
                        column: x => x.supplier_product_id,
                        principalSchema: "product_enrichment",
                        principalTable: "supplier_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "supplier_product_analogues_supplier_analogue_product_id_idx",
                schema: "product_enrichment",
                table: "supplier_product_analogues",
                column: "supplier_analogue_product_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.supplier.supplierproductmapping_who_created_idx",
                schema: "product_enrichment",
                table: "supplier_product_mappings",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.supplier.supplierproductmapping_who_updated_idx",
                schema: "product_enrichment",
                table: "supplier_product_mappings",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "supplier_product_mappings_product_supplier_product_uidx",
                schema: "product_enrichment",
                table: "supplier_product_mappings",
                columns: new[] { "product_id", "supplier_product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "supplier_product_mappings_status_idx",
                schema: "product_enrichment",
                table: "supplier_product_mappings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "supplier_product_mappings_supplier_product_id_idx",
                schema: "product_enrichment",
                table: "supplier_product_mappings",
                column: "supplier_product_id");

            migrationBuilder.CreateIndex(
                name: "supplier_product_names_product_supplier_name_uidx",
                schema: "product_enrichment",
                table: "supplier_product_names",
                columns: new[] { "supplier_product_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "main.entities.product.supplier.supplierproduct_who_created_idx",
                schema: "product_enrichment",
                table: "supplier_products",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.supplier.supplierproduct_who_updated_idx",
                schema: "product_enrichment",
                table: "supplier_products",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "supplier_products_producer_idx",
                schema: "product_enrichment",
                table: "supplier_products",
                column: "producer");

            migrationBuilder.CreateIndex(
                name: "supplier_products_normalized_sku_producer_uidx",
                schema: "product_enrichment",
                table: "supplier_products",
                columns: new[] { "normalized_sku", "producer" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_product_analogues",
                schema: "product_enrichment");

            migrationBuilder.DropTable(
                name: "supplier_product_mappings",
                schema: "product_enrichment");

            migrationBuilder.DropTable(
                name: "supplier_product_names",
                schema: "product_enrichment");

            migrationBuilder.DropTable(
                name: "supplier_products",
                schema: "product_enrichment");
        }
    }
}

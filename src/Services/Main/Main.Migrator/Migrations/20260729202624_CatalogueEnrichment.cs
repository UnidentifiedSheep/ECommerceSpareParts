using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class CatalogueEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_product_analogues",
                schema: "product_enrichment");

            migrationBuilder.DropTable(
                name: "supplier_product_mappings",
                schema: "product_enrichment");

            migrationBuilder.DropIndex(
                name: "sale_content_product_id_index",
                schema: "public",
                table: "sale_content");

            migrationBuilder.DropIndex(
                name: "supplier_products_producer_idx",
                schema: "product_enrichment",
                table: "supplier_products");

            migrationBuilder.DropColumn(
                name: "supplier",
                schema: "product_enrichment",
                table: "supplier_product_names");

            migrationBuilder.EnsureSchema(
                name: "catalogue_enrichment");

            migrationBuilder.RenameTable(
                name: "supplier_products",
                schema: "product_enrichment",
                newName: "supplier_products",
                newSchema: "catalogue_enrichment");

            migrationBuilder.RenameTable(
                name: "supplier_product_names",
                schema: "product_enrichment",
                newName: "supplier_product_names",
                newSchema: "catalogue_enrichment");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.supplier.supplierproduct_who_updated_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                newName: "main.entities.product.enrichment.supplierproduct_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.supplier.supplierproduct_who_created_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                newName: "main.entities.product.enrichment.supplierproduct_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "supplier_product_names_product_supplier_name_uidx",
                schema: "catalogue_enrichment",
                table: "supplier_product_names",
                newName: "supplier_product_names_product_name_uidx");

            migrationBuilder.AlterColumn<string>(
                name: "producer",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<int>(
                name: "catalogue_candidate_id",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "supplier",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "catalogue_enrichment",
                table: "supplier_product_names",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "catalogue_candidates",
                schema: "catalogue_enrichment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    normalized_sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("catalogue_candidates_pk", x => x.id);
                    table.ForeignKey(
                        name: "catalogue_candidates_producer_id_fk",
                        column: x => x.producer_id,
                        principalSchema: "public",
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "catalogue_candidates_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "sale_content_product_id_sale_id_index",
                schema: "public",
                table: "sale_content",
                columns: new[] { "product_id", "sale_id" });

            migrationBuilder.CreateIndex(
                name: "supplier_products_catalogue_candidate_id_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                column: "catalogue_candidate_id");

            migrationBuilder.CreateIndex(
                name: "catalogue_candidates_product_id_uidx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                column: "product_id",
                unique: true,
                filter: "product_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_catalogue_candidates_producer_id",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.enrichment.cataloguecandidate_who_created_idx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.enrichment.cataloguecandidate_who_updated_idx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                column: "who_updated");

            migrationBuilder.AddForeignKey(
                name: "supplier_products_catalogue_candidate_id_fk",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                column: "catalogue_candidate_id",
                principalSchema: "catalogue_enrichment",
                principalTable: "catalogue_candidates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropIndex(
                name: "supplier_products_normalized_sku_producer_uidx",
                schema: "product_enrichment",
                table: "supplier_products");

            migrationBuilder.CreateIndex(
                name: "catalogue_candidates_sku_producer_uidx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                columns: new[] { "normalized_sku", "producer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "supplier_products_normalized_sku_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                column: "normalized_sku")
                .Annotation("MaxLength", 128)
                .Annotation("Relational:ColumnName", "normalized_sku");

            migrationBuilder.CreateIndex(
                name: "supplier_products_supplier_producer_sku_uidx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                columns: new[] { "supplier", "producer", "normalized_sku" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "supplier_products_catalogue_candidate_id_fk",
                schema: "catalogue_enrichment",
                table: "supplier_products");

            migrationBuilder.DropTable(
                name: "catalogue_candidates",
                schema: "catalogue_enrichment");

            migrationBuilder.DropIndex(
                name: "sale_content_product_id_sale_id_index",
                schema: "public",
                table: "sale_content");

            migrationBuilder.DropIndex(
                name: "supplier_products_catalogue_candidate_id_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products");

            migrationBuilder.DropColumn(
                name: "catalogue_candidate_id",
                schema: "catalogue_enrichment",
                table: "supplier_products");

            migrationBuilder.DropColumn(
                name: "supplier",
                schema: "catalogue_enrichment",
                table: "supplier_products");

            migrationBuilder.EnsureSchema(
                name: "product_enrichment");

            migrationBuilder.RenameTable(
                name: "supplier_products",
                schema: "catalogue_enrichment",
                newName: "supplier_products",
                newSchema: "product_enrichment");

            migrationBuilder.RenameTable(
                name: "supplier_product_names",
                schema: "catalogue_enrichment",
                newName: "supplier_product_names",
                newSchema: "product_enrichment");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.enrichment.supplierproduct_who_updated_idx",
                schema: "product_enrichment",
                table: "supplier_products",
                newName: "main.entities.product.supplier.supplierproduct_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.enrichment.supplierproduct_who_created_idx",
                schema: "product_enrichment",
                table: "supplier_products",
                newName: "main.entities.product.supplier.supplierproduct_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "supplier_product_names_product_name_uidx",
                schema: "product_enrichment",
                table: "supplier_product_names",
                newName: "supplier_product_names_product_supplier_name_uidx");

            migrationBuilder.AlterColumn<string>(
                name: "producer",
                schema: "product_enrichment",
                table: "supplier_products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "product_enrichment",
                table: "supplier_product_names",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "supplier",
                schema: "product_enrichment",
                table: "supplier_product_names",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    supplier_product_id = table.Column<int>(type: "integer", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "sale_content_product_id_index",
                schema: "public",
                table: "sale_content",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "supplier_products_producer_idx",
                schema: "product_enrichment",
                table: "supplier_products",
                column: "producer");

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

            migrationBuilder.DropIndex(
                name: "supplier_products_normalized_sku_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products");

            migrationBuilder.DropIndex(
                name: "supplier_products_supplier_producer_sku_uidx",
                schema: "catalogue_enrichment",
                table: "supplier_products");

            migrationBuilder.CreateIndex(
                name: "supplier_products_normalized_sku_producer_uidx",
                schema: "product_enrichment",
                table: "supplier_products",
                columns: new[] { "normalized_sku", "producer" },
                unique: true);
        }
    }
}

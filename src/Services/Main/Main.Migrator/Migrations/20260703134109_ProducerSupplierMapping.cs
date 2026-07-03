using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ProducerSupplierMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "producers_other_names",
                schema: "public");

            migrationBuilder.RenameIndex(
                name: "main.entities.setting.storagecontentsetting_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.favoritsuppliersetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.setting.storagecontentsetting_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.favoritsuppliersetting_who_created_idx");

            migrationBuilder.CreateTable(
                name: "producer_supplier_mappings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    supplier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    producer_supplier_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("producer_supplier_mappings_pk", x => x.id);
                    table.ForeignKey(
                        name: "producer_supplier_mappings_producer_id_fk",
                        column: x => x.producer_id,
                        principalSchema: "public",
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "producers_aliases",
                schema: "public",
                columns: table => new
                {
                    other_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    producer_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("producers_other_names_pk", x => x.other_name);
                    table.ForeignKey(
                        name: "producers_other_names_producer_id_fk",
                        column: x => x.producer_id,
                        principalSchema: "public",
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "producer_supplier_mappings_uidx",
                schema: "public",
                table: "producer_supplier_mappings",
                columns: new[] { "producer_id", "supplier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_id_index",
                schema: "public",
                table: "producers_aliases",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_other_name_index",
                schema: "public",
                table: "producers_aliases",
                column: "other_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "producer_supplier_mappings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "producers_aliases",
                schema: "public");

            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.favoritsuppliersetting_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.setting.storagecontentsetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.favoritsuppliersetting_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.setting.storagecontentsetting_who_created_idx");

            migrationBuilder.CreateTable(
                name: "producers_other_names",
                schema: "public",
                columns: table => new
                {
                    other_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    where_used = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("producers_other_names_pk", x => x.other_name);
                    table.ForeignKey(
                        name: "producers_other_names_producer_id_fk",
                        column: x => x.producer_id,
                        principalSchema: "public",
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_id_index",
                schema: "public",
                table: "producers_other_names",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_other_name_index",
                schema: "public",
                table: "producers_other_names",
                column: "other_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "producers_other_names_where_used_index",
                schema: "public",
                table: "producers_other_names",
                column: "where_used")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProducerSupplierMappingUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "producer_supplier_mappings_uidx",
                schema: "public",
                table: "producer_supplier_mappings");

            migrationBuilder.Sql(
                """
                UPDATE public.producer_supplier_mappings
                SET producer_supplier_name = BTRIM(producer_supplier_name)
                WHERE producer_supplier_name <> BTRIM(producer_supplier_name);
                """);

            migrationBuilder.AddUniqueConstraint(
                name: "producer_supplier_mappings_supplier_name_key",
                schema: "public",
                table: "producer_supplier_mappings",
                columns: new[] { "producer_supplier_name", "supplier" });

            migrationBuilder.CreateIndex(
                name: "IX_producer_supplier_mappings_producer_id",
                schema: "public",
                table: "producer_supplier_mappings",
                column: "producer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "producer_supplier_mappings_supplier_name_key",
                schema: "public",
                table: "producer_supplier_mappings");

            migrationBuilder.DropIndex(
                name: "IX_producer_supplier_mappings_producer_id",
                schema: "public",
                table: "producer_supplier_mappings");

            migrationBuilder.CreateIndex(
                name: "producer_supplier_mappings_uidx",
                schema: "public",
                table: "producer_supplier_mappings",
                columns: new[] { "producer_id", "supplier" },
                unique: true);
        }
    }
}

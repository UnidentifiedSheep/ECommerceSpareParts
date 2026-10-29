using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeProducerSupplierMappingName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "producer_supplier_mappings_supplier_name_key",
                schema: "public",
                table: "producer_supplier_mappings");

            migrationBuilder.AddColumn<string>(
                name: "normalized_producer_supplier_name",
                schema: "public",
                table: "producer_supplier_mappings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE public.producer_supplier_mappings
                SET normalized_producer_supplier_name = UPPER(BTRIM(producer_supplier_name));
                """);

            migrationBuilder.AlterColumn<string>(
                name: "normalized_producer_supplier_name",
                schema: "public",
                table: "producer_supplier_mappings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "producer_supplier_mappings_supplier_name_key",
                schema: "public",
                table: "producer_supplier_mappings",
                columns: new[] { "normalized_producer_supplier_name", "supplier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "producer_supplier_mappings_supplier_name_key",
                schema: "public",
                table: "producer_supplier_mappings");

            migrationBuilder.DropColumn(
                name: "normalized_producer_supplier_name",
                schema: "public",
                table: "producer_supplier_mappings");

            migrationBuilder.AddUniqueConstraint(
                name: "producer_supplier_mappings_supplier_name_key",
                schema: "public",
                table: "producer_supplier_mappings",
                columns: new[] { "producer_supplier_name", "supplier" });
        }
    }
}

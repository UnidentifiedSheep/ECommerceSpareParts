using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Characteristics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "product_characteristics_pk",
                schema: "public",
                table: "product_characteristics");

            migrationBuilder.DropIndex(
                name: "product_characteristics_name_value_index",
                schema: "public",
                table: "product_characteristics");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "public",
                table: "product_characteristics");

            migrationBuilder.AddColumn<int>(
                name: "characteristic_id",
                schema: "public",
                table: "product_characteristics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "product_characteristics_pk",
                schema: "public",
                table: "product_characteristics",
                columns: new[] { "product_id", "characteristic_id" });

            migrationBuilder.CreateTable(
                name: "characteristics",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("characteristics_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "product_characteristics_characteristic_id_value_index",
                schema: "public",
                table: "product_characteristics",
                columns: new[] { "characteristic_id", "value" });

            migrationBuilder.CreateIndex(
                name: "characteristics_normalized_name_uindex",
                schema: "public",
                table: "characteristics",
                column: "normalized_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "product_characteristics_characteristic_id_fk",
                schema: "public",
                table: "product_characteristics",
                column: "characteristic_id",
                principalSchema: "public",
                principalTable: "characteristics",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "product_characteristics_characteristic_id_fk",
                schema: "public",
                table: "product_characteristics");

            migrationBuilder.DropTable(
                name: "characteristics",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "product_characteristics_pk",
                schema: "public",
                table: "product_characteristics");

            migrationBuilder.DropIndex(
                name: "product_characteristics_characteristic_id_value_index",
                schema: "public",
                table: "product_characteristics");

            migrationBuilder.DropColumn(
                name: "characteristic_id",
                schema: "public",
                table: "product_characteristics");

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "public",
                table: "product_characteristics",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "product_characteristics_pk",
                schema: "public",
                table: "product_characteristics",
                columns: new[] { "product_id", "name" });

            migrationBuilder.CreateIndex(
                name: "product_characteristics_name_value_index",
                schema: "public",
                table: "product_characteristics",
                columns: new[] { "name", "value" });
        }
    }
}

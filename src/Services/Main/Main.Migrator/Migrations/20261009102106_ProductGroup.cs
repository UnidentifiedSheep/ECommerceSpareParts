using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ProductGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "products_categories_id_fk",
                schema: "public",
                table: "products");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "public");

            migrationBuilder.AddColumn<int>(
                name: "product_group_id",
                schema: "public",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "product_groups",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_groups_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "products_product_group_id_index",
                schema: "public",
                table: "products",
                column: "product_group_id");

            migrationBuilder.CreateIndex(
                name: "product_groups_normalized_name_uindex",
                schema: "public",
                table: "product_groups",
                column: "normalized_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "products_product_group_id_fk",
                schema: "public",
                table: "products",
                column: "product_group_id",
                principalSchema: "public",
                principalTable: "product_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "products_product_group_id_fk",
                schema: "public",
                table: "products");

            migrationBuilder.DropTable(
                name: "product_groups",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "products_product_group_id_index",
                schema: "public",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_group_id",
                schema: "public",
                table: "products");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("categories_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "categories_name_index",
                schema: "public",
                table: "categories",
                column: "name");

            migrationBuilder.AddForeignKey(
                name: "products_categories_id_fk",
                schema: "public",
                table: "products",
                column: "category_id",
                principalSchema: "public",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

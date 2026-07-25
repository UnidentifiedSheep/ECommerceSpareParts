using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ProductImageUpd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "public",
                table: "product_images");

            migrationBuilder.RenameColumn(
                name: "path",
                schema: "public",
                table: "product_images",
                newName: "storage_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "storage_key",
                schema: "public",
                table: "product_images",
                newName: "path");

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "public",
                table: "product_images",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }
    }
}

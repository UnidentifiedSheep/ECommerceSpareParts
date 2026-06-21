using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class StorageContentPositiveCountIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "storage_content_product_storage_positive_count_idx",
                schema: "public",
                table: "storage_content",
                columns: new[] { "product_id", "storage_name" },
                filter: "(count > 0)")
                .Annotation("Npgsql:IndexInclude", new[] { "count" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "storage_content_product_storage_positive_count_idx",
                schema: "public",
                table: "storage_content");
        }
    }
}

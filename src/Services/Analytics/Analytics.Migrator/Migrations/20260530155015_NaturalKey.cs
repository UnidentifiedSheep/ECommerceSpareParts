using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class NaturalKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "metrics_range_start_end_discriminator_u_index",
                table: "metrics");

            migrationBuilder.RenameColumn(
                name: "dimension_hash",
                table: "metrics",
                newName: "natural_key");

            migrationBuilder.CreateIndex(
                name: "metrics_natural_key_index",
                table: "metrics",
                column: "natural_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "metrics_natural_key_index",
                table: "metrics");

            migrationBuilder.RenameColumn(
                name: "natural_key",
                table: "metrics",
                newName: "dimension_hash");

            migrationBuilder.CreateIndex(
                name: "metrics_range_start_end_discriminator_u_index",
                table: "metrics",
                columns: new[] { "discriminator", "range_start", "range_end", "dimension_hash" },
                unique: true);
        }
    }
}

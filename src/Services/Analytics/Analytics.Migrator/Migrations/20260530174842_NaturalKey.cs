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

            migrationBuilder.DropIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs");

            migrationBuilder.RenameColumn(
                name: "dimension_hash",
                table: "metrics",
                newName: "natural_key");

            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                table: "metrics",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);

            migrationBuilder.CreateIndex(
                name: "metrics_natural_key_index",
                table: "metrics",
                column: "natural_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "metrics_natural_key_index",
                table: "metrics");

            migrationBuilder.DropIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs");

            migrationBuilder.RenameColumn(
                name: "natural_key",
                table: "metrics",
                newName: "dimension_hash");

            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                table: "metrics",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34);

            migrationBuilder.CreateIndex(
                name: "metrics_range_start_end_discriminator_u_index",
                table: "metrics",
                columns: new[] { "discriminator", "range_start", "range_end", "dimension_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id",
                unique: true);
        }
    }
}

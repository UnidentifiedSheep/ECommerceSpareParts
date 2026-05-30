using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class NonUniqMetricId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs");

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs");

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id",
                unique: true);
        }
    }
}

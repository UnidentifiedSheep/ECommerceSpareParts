using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM job.job_schedules
                WHERE job_system_name = 'MetricCalculationLrt';

                DELETE FROM job.jobs
                WHERE system_name = 'MetricCalculationLrt';
                """);

            migrationBuilder.DropTable(
                name: "metric_jobs");

            migrationBuilder.DropTable(
                name: "metrics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    depends_on = table.Column<long>(type: "bigint", nullable: false),
                    dimension_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    json = table.Column<string>(type: "text", nullable: true),
                    natural_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    range_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    range_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recalculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tags = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("metrics_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "metric_jobs",
                columns: table => new
                {
                    metric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("metric_jobs_pk", x => new { x.metric_id, x.job_id });
                    table.ForeignKey(
                        name: "metric_jobs_job_id_fk",
                        column: x => x.job_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "metric_jobs_metric_id_fk",
                        column: x => x.metric_id,
                        principalTable: "metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "metric_jobs_job_id_idx",
                table: "metric_jobs",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "metric_jobs_metric_id_idx",
                table: "metric_jobs",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "analytics.entities.metrics.productsalesmetric_who_created_idx",
                table: "metrics",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "analytics.entities.metrics.productsalesmetric_who_updated_idx",
                table: "metrics",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "metrics_currency_id_index",
                table: "metrics",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "metrics_dirty_index",
                table: "metrics",
                column: "discriminator",
                filter: "(tags & 1) = 1");

            migrationBuilder.CreateIndex(
                name: "metrics_discriminator_article_index",
                table: "metrics",
                columns: new[] { "discriminator", "product_id" });

            migrationBuilder.CreateIndex(
                name: "metrics_natural_key_index",
                table: "metrics",
                column: "natural_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "metrics_range_depends_index",
                table: "metrics",
                columns: new[] { "depends_on", "range_start", "range_end" });
        }
    }
}

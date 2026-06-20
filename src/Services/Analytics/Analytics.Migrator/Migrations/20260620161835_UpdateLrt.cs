using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLrt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metric_calculation_jobs");

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    locked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("jobs_pk", x => x.id);
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
                name: "domain.commonentities.job_who_created_idx",
                table: "jobs",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.job_who_updated_idx",
                table: "jobs",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "jobs_locked_at_idx",
                table: "jobs",
                column: "locked_at");

            migrationBuilder.CreateIndex(
                name: "jobs_status_id_idx",
                table: "jobs",
                columns: new[] { "status", "id" });

            migrationBuilder.CreateIndex(
                name: "jobs_system_name_idx",
                table: "jobs",
                column: "system_name");

            migrationBuilder.CreateIndex(
                name: "metric_jobs_job_id_idx",
                table: "metric_jobs",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "metric_jobs_metric_id_idx",
                table: "metric_jobs",
                column: "metric_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metric_jobs");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.CreateTable(
                name: "metric_calculation_jobs",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    error_message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    metric_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metric_system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    status = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("request_id_pk", x => x.request_id);
                    table.ForeignKey(
                        name: "metric_calculation_jobs_metric_id_fk",
                        column: x => x.metric_id,
                        principalTable: "metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "analytics.entities.metriccalculationjob_who_created_idx",
                table: "metric_calculation_jobs",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "analytics.entities.metriccalculationjob_who_updated_idx",
                table: "metric_calculation_jobs",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_status_name_index",
                table: "metric_calculation_jobs",
                columns: new[] { "status", "metric_system_name" });
        }
    }
}

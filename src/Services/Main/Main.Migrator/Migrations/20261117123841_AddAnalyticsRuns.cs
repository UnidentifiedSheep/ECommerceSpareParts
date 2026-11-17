using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_runs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    analysis_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false),
                    parameters = table.Column<string>(type: "jsonb", nullable: false),
                    parameters_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    summary = table.Column<string>(type: "jsonb", nullable: true),
                    items_count = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("analytics_runs_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analytics_run_items",
                schema: "public",
                columns: table => new
                {
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: true),
                    value = table.Column<decimal>(type: "numeric", nullable: true),
                    share = table.Column<decimal>(type: "numeric", nullable: true),
                    segments = table.Column<string>(type: "jsonb", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("analytics_run_items_pk", x => new { x.run_id, x.subject_key });
                    table.ForeignKey(
                        name: "analytics_run_items_run_id_fk",
                        column: x => x.run_id,
                        principalSchema: "public",
                        principalTable: "analytics_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "analytics_run_items_run_id_rank_index",
                schema: "public",
                table: "analytics_run_items",
                columns: new[] { "run_id", "rank" });

            migrationBuilder.CreateIndex(
                name: "analytics_run_items_segments_index",
                schema: "public",
                table: "analytics_run_items",
                column: "segments")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "jsonb_path_ops" });

            migrationBuilder.CreateIndex(
                name: "analytics_runs_analysis_type_parameters_hash_index",
                schema: "public",
                table: "analytics_runs",
                columns: new[] { "analysis_type", "parameters_hash" });

            migrationBuilder.CreateIndex(
                name: "analytics_runs_analysis_type_period_end_index",
                schema: "public",
                table: "analytics_runs",
                columns: new[] { "analysis_type", "period_end" });

            migrationBuilder.CreateIndex(
                name: "analytics_runs_job_id_index",
                schema: "public",
                table: "analytics_runs",
                column: "job_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_run_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "analytics_runs",
                schema: "public");
        }
    }
}

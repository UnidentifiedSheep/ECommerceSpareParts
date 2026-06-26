using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class JobSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "job");

            migrationBuilder.RenameTable(
                name: "jobs",
                schema: "public",
                newName: "jobs",
                newSchema: "job");

            migrationBuilder.CreateTable(
                name: "job_schedules",
                schema: "job",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    job_system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    input_state = table.Column<string>(type: "text", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    cron = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_queued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("job_schedules_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_schedule_runs",
                schema: "job",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    queued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("job_schedule_runs_pk", x => x.id);
                    table.ForeignKey(
                        name: "job_schedule_runs_job_id_fk",
                        column: x => x.job_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "job_schedule_runs_job_schedule_id_fk",
                        column: x => x.job_schedule_id,
                        principalSchema: "job",
                        principalTable: "job_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "job_schedule_runs_job_id_idx",
                schema: "job",
                table: "job_schedule_runs",
                column: "job_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "job_schedule_runs_job_schedule_id_scheduled_at_idx",
                schema: "job",
                table: "job_schedule_runs",
                columns: new[] { "job_schedule_id", "scheduled_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.jobschedule_who_created_idx",
                schema: "job",
                table: "job_schedules",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.jobschedule_who_updated_idx",
                schema: "job",
                table: "job_schedules",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "job_schedules_enabled_next_run_at_id_idx",
                schema: "job",
                table: "job_schedules",
                columns: new[] { "enabled", "next_run_at", "id" });

            migrationBuilder.CreateIndex(
                name: "job_schedules_job_system_name_idx",
                schema: "job",
                table: "job_schedules",
                column: "job_system_name");

            migrationBuilder.CreateIndex(
                name: "job_schedules_name_idx",
                schema: "job",
                table: "job_schedules",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_schedule_runs",
                schema: "job");

            migrationBuilder.DropTable(
                name: "job_schedules",
                schema: "job");

            migrationBuilder.RenameTable(
                name: "jobs",
                schema: "job",
                newName: "jobs",
                newSchema: "public");
        }
    }
}

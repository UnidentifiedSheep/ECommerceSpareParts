using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class JobSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.jobschedule_who_created_idx",
                table: "job_schedules",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.jobschedule_who_updated_idx",
                table: "job_schedules",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "job_schedules_enabled_next_run_at_id_idx",
                table: "job_schedules",
                columns: new[] { "enabled", "next_run_at", "id" });

            migrationBuilder.CreateIndex(
                name: "job_schedules_system_name_idx",
                table: "job_schedules",
                column: "system_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_schedules");
        }
    }
}

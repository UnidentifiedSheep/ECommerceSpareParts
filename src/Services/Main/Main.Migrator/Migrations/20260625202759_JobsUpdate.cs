using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class JobsUpdate : Migration
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

            migrationBuilder.RenameTable(
                name: "job_schedules",
                schema: "public",
                newName: "job_schedules",
                newSchema: "job");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_schedule_runs",
                schema: "job");

            migrationBuilder.RenameTable(
                name: "jobs",
                schema: "job",
                newName: "jobs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "job_schedules",
                schema: "job",
                newName: "job_schedules",
                newSchema: "public");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class MultiStepJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "domain.commonentities.uniqjob_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.uniqjob_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.uniqjob_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.uniqjob_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.jobschedule_who_updated_idx",
                schema: "job",
                table: "job_schedules",
                newName: "domain.commonentities.job.jobschedule_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.jobschedule_who_created_idx",
                schema: "job",
                table: "job_schedules",
                newName: "domain.commonentities.job.jobschedule_who_created_idx");

            migrationBuilder.AlterColumn<string>(
                name: "job_type",
                schema: "job",
                table: "jobs",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<Guid>(
                name: "multi_step_job_id",
                schema: "job",
                table: "jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "job_step_dependencies",
                schema: "job",
                columns: table => new
                {
                    step_id = table.Column<Guid>(type: "uuid", nullable: false),
                    depends_on_step_id = table.Column<Guid>(type: "uuid", nullable: false),
                    multi_step_job_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("job_step_dependencies_pk", x => new { x.step_id, x.depends_on_step_id });
                    table.ForeignKey(
                        name: "job_step_dependencies_depends_on_step_id_fk",
                        column: x => x.depends_on_step_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "job_step_dependencies_multi_step_job_id_fk",
                        column: x => x.multi_step_job_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "job_step_dependencies_step_id_fk",
                        column: x => x.step_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "jobs_multi_step_job_id_idx",
                schema: "job",
                table: "jobs",
                column: "multi_step_job_id");

            migrationBuilder.CreateIndex(
                name: "job_step_dependencies_depends_on_step_id_idx",
                schema: "job",
                table: "job_step_dependencies",
                column: "depends_on_step_id");

            migrationBuilder.CreateIndex(
                name: "job_step_dependencies_multi_step_job_id_idx",
                schema: "job",
                table: "job_step_dependencies",
                column: "multi_step_job_id");

            migrationBuilder.AddForeignKey(
                name: "jobs_multi_step_job_id_fk",
                schema: "job",
                table: "jobs",
                column: "multi_step_job_id",
                principalSchema: "job",
                principalTable: "jobs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM job.jobs
                WHERE job_type = 'multi_step_job';
                """);

            migrationBuilder.DropForeignKey(
                name: "jobs_multi_step_job_id_fk",
                schema: "job",
                table: "jobs");

            migrationBuilder.DropTable(
                name: "job_step_dependencies",
                schema: "job");

            migrationBuilder.DropIndex(
                name: "jobs_multi_step_job_id_idx",
                schema: "job",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "multi_step_job_id",
                schema: "job",
                table: "jobs");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.uniqjob_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.uniqjob_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.uniqjob_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.uniqjob_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.jobschedule_who_updated_idx",
                schema: "job",
                table: "job_schedules",
                newName: "domain.commonentities.jobschedule_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.jobschedule_who_created_idx",
                schema: "job",
                table: "job_schedules",
                newName: "domain.commonentities.jobschedule_who_created_idx");

            migrationBuilder.AlterColumn<string>(
                name: "job_type",
                schema: "job",
                table: "jobs",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);
        }
    }
}

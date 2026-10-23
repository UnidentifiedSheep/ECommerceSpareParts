using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class DeferredJobInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "input_initialized",
                schema: "job",
                table: "jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "output",
                schema: "job",
                table: "jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "step_key",
                schema: "job",
                table: "jobs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "jobs_multi_step_job_id_step_key_uq",
                schema: "job",
                table: "jobs",
                columns: new[] { "multi_step_job_id", "step_key" },
                unique: true,
                filter: "step_key IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "jobs_multi_step_job_id_step_key_uq",
                schema: "job",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "input_initialized",
                schema: "job",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "output",
                schema: "job",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "step_key",
                schema: "job",
                table: "jobs");
        }
    }
}

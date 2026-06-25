using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "system_name",
                schema: "public",
                table: "job_schedules",
                newName: "job_system_name");

            migrationBuilder.RenameIndex(
                name: "job_schedules_system_name_idx",
                schema: "public",
                table: "job_schedules",
                newName: "job_schedules_job_system_name_idx");

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "public",
                table: "job_schedules",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "public",
                table: "job_schedules",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "job_schedules_name_idx",
                schema: "public",
                table: "job_schedules",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "job_schedules_name_idx",
                schema: "public",
                table: "job_schedules");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "public",
                table: "job_schedules");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "public",
                table: "job_schedules");

            migrationBuilder.RenameColumn(
                name: "job_system_name",
                schema: "public",
                table: "job_schedules",
                newName: "system_name");

            migrationBuilder.RenameIndex(
                name: "job_schedules_job_system_name_idx",
                schema: "public",
                table: "job_schedules",
                newName: "job_schedules_system_name_idx");
        }
    }
}

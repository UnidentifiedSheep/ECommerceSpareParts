using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class JobUpd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.uniqjob_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.uniqjob_who_created_idx");

            migrationBuilder.AddColumn<string>(
                name: "job_type",
                schema: "job",
                table: "jobs",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "natural_key",
                schema: "job",
                table: "jobs",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "jobs_pending_system_name_natural_key_uq",
                schema: "job",
                table: "jobs",
                columns: new[] { "system_name", "natural_key" },
                unique: true,
                filter: "status = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "jobs_pending_system_name_natural_key_uq",
                schema: "job",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "job_type",
                schema: "job",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "natural_key",
                schema: "job",
                table: "jobs");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.uniqjob_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.uniqjob_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job_who_created_idx");
        }
    }
}

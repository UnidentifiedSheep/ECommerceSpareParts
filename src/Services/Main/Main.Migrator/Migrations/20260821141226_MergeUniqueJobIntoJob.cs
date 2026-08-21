using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class MergeUniqueJobIntoJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE job.jobs SET job_type = 'job' WHERE job_type = 'uniq_job';");

            migrationBuilder.DropIndex(
                name: "jobs_pending_system_name_natural_key_uq",
                schema: "job",
                table: "jobs");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.uniqjob_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.singlerunjob_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.uniqjob_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.singlerunjob_who_created_idx");

            migrationBuilder.CreateIndex(
                name: "jobs_pending_system_name_natural_key_uq",
                schema: "job",
                table: "jobs",
                columns: new[] { "system_name", "natural_key" },
                unique: true,
                filter: "status = 'Pending' AND natural_key IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE job.jobs SET job_type = 'uniq_job' " +
                "WHERE job_type = 'job' AND natural_key IS NOT NULL;");

            migrationBuilder.DropIndex(
                name: "jobs_pending_system_name_natural_key_uq",
                schema: "job",
                table: "jobs");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.singlerunjob_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.uniqjob_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.singlerunjob_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.uniqjob_who_created_idx");

            migrationBuilder.CreateIndex(
                name: "jobs_pending_system_name_natural_key_uq",
                schema: "job",
                table: "jobs",
                columns: new[] { "system_name", "natural_key" },
                unique: true,
                filter: "status = 'Pending'");
        }
    }
}

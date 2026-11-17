using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class JobReadyNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION job.notify_job_ready() RETURNS trigger AS $$
                BEGIN
                    PERFORM pg_notify('job_ready', NEW.id::text);
                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER jobs_notify_ready_on_insert
                AFTER INSERT ON job.jobs
                FOR EACH ROW
                WHEN (NEW.status = 'Pending')
                EXECUTE FUNCTION job.notify_job_ready();
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER jobs_notify_ready_on_update
                AFTER UPDATE OF status ON job.jobs
                FOR EACH ROW
                WHEN (NEW.status = 'Pending' AND OLD.status IS DISTINCT FROM NEW.status)
                EXECUTE FUNCTION job.notify_job_ready();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS jobs_notify_ready_on_update ON job.jobs;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS jobs_notify_ready_on_insert ON job.jobs;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS job.notify_job_ready();");
        }
    }
}

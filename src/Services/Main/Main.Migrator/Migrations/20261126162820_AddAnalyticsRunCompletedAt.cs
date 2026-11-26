using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsRunCompletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                schema: "public",
                table: "analytics_runs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE public.analytics_runs SET completed_at = created_at WHERE completed_at IS NULL;");

            migrationBuilder.CreateIndex(
                name: "analytics_runs_analysis_type_completed_at_index",
                schema: "public",
                table: "analytics_runs",
                columns: new[] { "analysis_type", "completed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "analytics_runs_analysis_type_completed_at_index",
                schema: "public",
                table: "analytics_runs");

            migrationBuilder.DropColumn(
                name: "completed_at",
                schema: "public",
                table: "analytics_runs");
        }
    }
}

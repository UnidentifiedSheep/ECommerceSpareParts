using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AnalyticsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "buyer_id",
                schema: "public",
                table: "sale_content_analytics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "count",
                schema: "public",
                table: "sale_content_analytics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_completed",
                schema: "public",
                table: "sale_content_analytics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                schema: "public",
                table: "sale_content_analytics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "sale_datetime",
                schema: "public",
                table: "sale_content_analytics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "sale_id",
                schema: "public",
                table: "sale_content_analytics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE public.sale_content_analytics a
                SET sale_id = c.sale_id,
                    count = c.count,
                    organization_id = s.organization_id,
                    buyer_id = s.user_id,
                    sale_datetime = s.sale_datetime,
                    is_completed = (s.state = 'Completed')
                FROM public.sale_content c
                JOIN public.sale s ON s.id = c.sale_id
                WHERE a.sale_content_id = c.id;
                """);

            migrationBuilder.CreateIndex(
                name: "sale_content_analytics_buyer_id_index",
                schema: "public",
                table: "sale_content_analytics",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_analytics_is_completed_sale_datetime_index",
                schema: "public",
                table: "sale_content_analytics",
                columns: new[] { "is_completed", "sale_datetime" });

            migrationBuilder.CreateIndex(
                name: "sale_content_analytics_organization_id_index",
                schema: "public",
                table: "sale_content_analytics",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_analytics_sale_id_index",
                schema: "public",
                table: "sale_content_analytics",
                column: "sale_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "sale_content_analytics_buyer_id_index",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropIndex(
                name: "sale_content_analytics_is_completed_sale_datetime_index",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropIndex(
                name: "sale_content_analytics_organization_id_index",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropIndex(
                name: "sale_content_analytics_sale_id_index",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropColumn(
                name: "buyer_id",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropColumn(
                name: "count",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropColumn(
                name: "is_completed",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropColumn(
                name: "sale_datetime",
                schema: "public",
                table: "sale_content_analytics");

            migrationBuilder.DropColumn(
                name: "sale_id",
                schema: "public",
                table: "sale_content_analytics");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "price_recalculation_requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "price_recalculation_requests");
        }
    }
}

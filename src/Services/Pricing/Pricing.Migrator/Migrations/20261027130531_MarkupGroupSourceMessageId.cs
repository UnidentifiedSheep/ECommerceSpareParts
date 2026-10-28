using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class MarkupGroupSourceMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_message_id",
                table: "markup_group",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE markup_group
                SET source_message_id = gen_random_uuid()
                WHERE is_auto_generated;
                """);

            migrationBuilder.DropColumn(
                name: "is_auto_generated",
                table: "markup_group");

            migrationBuilder.CreateIndex(
                name: "markup_group_source_message_id_uindex",
                table: "markup_group",
                column: "source_message_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "markup_group_source_message_id_uindex",
                table: "markup_group");

            migrationBuilder.AddColumn<bool>(
                name: "is_auto_generated",
                table: "markup_group",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE markup_group
                SET is_auto_generated = source_message_id IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "source_message_id",
                table: "markup_group");
        }
    }
}

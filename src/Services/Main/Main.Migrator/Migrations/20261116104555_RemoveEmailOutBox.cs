using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmailOutBox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_outbox",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_outbox",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    subject = table.Column<string>(type: "text", nullable: false),
                    to = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_outbox", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "main.entities.mailing.emailoutbox_who_created_idx",
                schema: "public",
                table: "email_outbox",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.mailing.emailoutbox_who_updated_idx",
                schema: "public",
                table: "email_outbox",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "status_email_outbox_idx",
                schema: "public",
                table: "email_outbox",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "to_status_email_outbox_idx",
                schema: "public",
                table: "email_outbox",
                columns: new[] { "to", "status" });
        }
    }
}

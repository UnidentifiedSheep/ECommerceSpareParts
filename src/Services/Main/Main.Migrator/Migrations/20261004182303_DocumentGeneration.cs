using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class DocumentGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_generation_requests",
                schema: "public",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bucket_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    storage_key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("document_generation_requests_pk", x => x.request_id);
                    table.ForeignKey(
                        name: "document_generation_requests_job_fk",
                        column: x => x.job_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "document_generation_requests_requester_fk",
                        column: x => x.requester_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "document_generation_requests_expires_at_idx",
                schema: "public",
                table: "document_generation_requests",
                column: "expires_at_utc",
                filter: "expires_at_utc IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "document_generation_requests_job_id_uq",
                schema: "public",
                table: "document_generation_requests",
                column: "job_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "document_generation_requests_requester_created_idx",
                schema: "public",
                table: "document_generation_requests",
                columns: new[] { "requester_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_generation_requests",
                schema: "public");
        }
    }
}

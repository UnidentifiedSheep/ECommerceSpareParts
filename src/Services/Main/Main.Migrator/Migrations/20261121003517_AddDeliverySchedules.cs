using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliverySchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logistics");

            migrationBuilder.CreateTable(
                name: "delivery_schedules",
                schema: "logistics",
                columns: table => new
                {
                    supplier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    storage_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("delivery_schedules_pk", x => new { x.supplier, x.storage_code });
                    table.ForeignKey(
                        name: "delivery_schedules_storage_code_fk",
                        column: x => x.storage_code,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delivery_schedule_exceptions",
                schema: "logistics",
                columns: table => new
                {
                    supplier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    storage_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("delivery_schedule_exceptions_pk", x => new { x.supplier, x.storage_code, x.date });
                    table.ForeignKey(
                        name: "delivery_schedule_exceptions_delivery_schedules_fk",
                        columns: x => new { x.supplier, x.storage_code },
                        principalSchema: "logistics",
                        principalTable: "delivery_schedules",
                        principalColumns: new[] { "supplier", "storage_code" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "delivery_schedule_exceptions_date_index",
                schema: "logistics",
                table: "delivery_schedule_exceptions",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "delivery_schedules_storage_code_index",
                schema: "logistics",
                table: "delivery_schedules",
                column: "storage_code");

            migrationBuilder.CreateIndex(
                name: "delivery_schedules_who_created_idx",
                schema: "logistics",
                table: "delivery_schedules",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "delivery_schedules_who_updated_idx",
                schema: "logistics",
                table: "delivery_schedules",
                column: "who_updated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_schedule_exceptions",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "delivery_schedules",
                schema: "logistics");
        }
    }
}

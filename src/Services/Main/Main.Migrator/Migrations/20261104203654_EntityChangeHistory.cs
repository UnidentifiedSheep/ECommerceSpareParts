using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class EntityChangeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "entity_history",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_key = table.Column<string>(type: "text", nullable: false),
                    aggregate_type = table.Column<string>(type: "text", nullable: false),
                    aggregate_key = table.Column<string>(type: "text", nullable: false),
                    operation = table.Column<string>(type: "text", nullable: false),
                    changes = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    reasons = table.Column<string>(type: "jsonb", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    change_set_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("entity_history_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "entity_history_aggregate_type_aggregate_key_occurred_at_id_idx",
                schema: "audit",
                table: "entity_history",
                columns: new[] { "aggregate_type", "aggregate_key", "occurred_at", "id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "entity_history_change_set_id_idx",
                schema: "audit",
                table: "entity_history",
                column: "change_set_id");

            migrationBuilder.CreateIndex(
                name: "entity_history_entity_type_entity_key_occurred_at_id_idx",
                schema: "audit",
                table: "entity_history",
                columns: new[] { "entity_type", "entity_key", "occurred_at", "id" },
                descending: new[] { false, false, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_history",
                schema: "audit");
        }
    }
}

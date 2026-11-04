using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAuditAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_history",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "events",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "entity_history",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aggregate_key = table.Column<string>(type: "text", nullable: false),
                    aggregate_type = table.Column<string>(type: "text", nullable: false),
                    change_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changes = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    entity_key = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    operation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("entity_history_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    json = table.Column<string>(type: "jsonb", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("events_id_pk", x => x.id);
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

            migrationBuilder.CreateIndex(
                name: "event_discriminator_idx",
                schema: "public",
                table: "events",
                column: "discriminator");

            migrationBuilder.CreateIndex(
                name: "main.entities.event.storagemovementevent_who_created_idx",
                schema: "public",
                table: "events",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.event.storagemovementevent_who_updated_idx",
                schema: "public",
                table: "events",
                column: "who_updated");
        }
    }
}

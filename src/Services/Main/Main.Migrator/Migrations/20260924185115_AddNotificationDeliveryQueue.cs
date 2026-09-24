using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDeliveryQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.CreateTable(
                name: "in_app_notifications",
                schema: "notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("in_app_notifications_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    model = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notifications_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_preferences",
                schema: "auth",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_notification_preferences_pk", x => new { x.user_id, x.channel_name });
                    table.ForeignKey(
                        name: "user_notification_preferences_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                schema: "notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "integer", nullable: false),
                    channel_system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    recipient_json = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notification_deliveries_pk", x => new { x.notification_id, x.channel_system_name });
                    table.ForeignKey(
                        name: "notification_deliveries_notification_id_fk",
                        column: x => x.notification_id,
                        principalSchema: "notification",
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "in_app_notifications_user_id_created_at_idx",
                schema: "notification",
                table: "in_app_notifications",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "notification_deliveries_channel_status_notification_id_idx",
                schema: "notification",
                table: "notification_deliveries",
                columns: new[] { "channel_system_name", "status", "notification_id" });

            migrationBuilder.CreateIndex(
                name: "notifications_user_id_created_at_idx",
                schema: "notification",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "in_app_notifications",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "notification_deliveries",
                schema: "notification");

            migrationBuilder.DropTable(
                name: "user_notification_preferences",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notification");
        }
    }
}

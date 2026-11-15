using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_vehicles",
                schema: "public");

            migrationBuilder.Sql("""
                DELETE FROM auth.role_permissions
                WHERE permission IN ('USERS.VEHICLES.CREATE.ME', 'USERS.VEHICLES.CREATE.ALL');
                DELETE FROM auth.permissions
                WHERE name IN ('USERS.VEHICLES.CREATE.ME', 'USERS.VEHICLES.CREATE.ALL');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_vehicles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    plate_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_vehicles_pk", x => x.id);
                    table.ForeignKey(
                        name: "user_vehicles_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "main.entities.user.uservehicle_who_created_idx",
                schema: "public",
                table: "user_vehicles",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.uservehicle_who_updated_idx",
                schema: "public",
                table: "user_vehicles",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_vehicles_comment_index",
                schema: "public",
                table: "user_vehicles",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_plate_number_uindex",
                schema: "public",
                table: "user_vehicles",
                column: "plate_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_vehicles_user_id_index",
                schema: "public",
                table: "user_vehicles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_vehicles_vehicle_id_index",
                schema: "public",
                table: "user_vehicles",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "user_vehicles_vin_uindex",
                schema: "public",
                table: "user_vehicles",
                column: "vin",
                unique: true);
        }
    }
}

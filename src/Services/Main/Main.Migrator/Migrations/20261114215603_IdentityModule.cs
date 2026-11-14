using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class IdentityModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_search_history",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_search_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    query = table.Column<string>(type: "jsonb", nullable: false),
                    search_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    search_place = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_search_history_pk", x => x.id);
                    table.ForeignKey(
                        name: "user_search_history_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "user_search_history_search_date_time_index",
                schema: "public",
                table: "user_search_history",
                column: "search_date_time");

            migrationBuilder.CreateIndex(
                name: "user_search_history_search_place_index",
                schema: "public",
                table: "user_search_history",
                column: "search_place");

            migrationBuilder.CreateIndex(
                name: "user_search_history_user_id_search_place_index",
                schema: "public",
                table: "user_search_history",
                columns: new[] { "user_id", "search_place" });
        }
    }
}

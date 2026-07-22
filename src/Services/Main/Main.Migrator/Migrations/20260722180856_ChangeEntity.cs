using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "storage_content_reservations_users_id_fk_2",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.DropForeignKey(
                name: "storage_content_reservations_users_id_fk_3",
                schema: "public",
                table: "storage_content_reservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "storage_content_reservations_users_id_fk_2",
                schema: "public",
                table: "storage_content_reservations",
                column: "who_updated",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "storage_content_reservations_users_id_fk_3",
                schema: "public",
                table: "storage_content_reservations",
                column: "who_created",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}

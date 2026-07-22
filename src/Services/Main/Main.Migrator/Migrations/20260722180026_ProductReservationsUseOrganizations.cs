using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ProductReservationsUseOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "storage_content_reservations_users_id_fk",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "public",
                table: "storage_content_reservations",
                newName: "organization_id");

            migrationBuilder.RenameIndex(
                name: "storage_content_reservations_user_id_status_index",
                schema: "public",
                table: "storage_content_reservations",
                newName: "storage_content_reservations_organization_id_status_index");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storagecontentreservation_who_updated_idx",
                schema: "public",
                table: "storage_content_reservations",
                newName: "main.entities.storage.productreservation_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storagecontentreservation_who_created_idx",
                schema: "public",
                table: "storage_content_reservations",
                newName: "main.entities.storage.productreservation_who_created_idx");

            migrationBuilder.AddForeignKey(
                name: "storage_content_reservations_organization_id_fk",
                schema: "public",
                table: "storage_content_reservations",
                column: "organization_id",
                principalSchema: "auth",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "storage_content_reservations_organization_id_fk",
                schema: "public",
                table: "storage_content_reservations");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "public",
                table: "storage_content_reservations",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "storage_content_reservations_organization_id_status_index",
                schema: "public",
                table: "storage_content_reservations",
                newName: "storage_content_reservations_user_id_status_index");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.productreservation_who_updated_idx",
                schema: "public",
                table: "storage_content_reservations",
                newName: "main.entities.storage.storagecontentreservation_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.productreservation_who_created_idx",
                schema: "public",
                table: "storage_content_reservations",
                newName: "main.entities.storage.storagecontentreservation_who_created_idx");

            migrationBuilder.AddForeignKey(
                name: "storage_content_reservations_users_id_fk",
                schema: "public",
                table: "storage_content_reservations",
                column: "user_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

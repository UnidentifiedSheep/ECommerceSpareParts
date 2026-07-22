using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UseOrganizationsForTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "transactions_users_id_fk",
                schema: "public",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "transactions_users_id_fk_2",
                schema: "public",
                table: "transactions");

            migrationBuilder.AddForeignKey(
                name: "transactions_receiver_organization_id_fk",
                schema: "public",
                table: "transactions",
                column: "receiver_id",
                principalSchema: "auth",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "transactions_sender_organization_id_fk",
                schema: "public",
                table: "transactions",
                column: "sender_id",
                principalSchema: "auth",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "transactions_receiver_organization_id_fk",
                schema: "public",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "transactions_sender_organization_id_fk",
                schema: "public",
                table: "transactions");

            migrationBuilder.AddForeignKey(
                name: "transactions_users_id_fk",
                schema: "public",
                table: "transactions",
                column: "sender_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "transactions_users_id_fk_2",
                schema: "public",
                table: "transactions",
                column: "receiver_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

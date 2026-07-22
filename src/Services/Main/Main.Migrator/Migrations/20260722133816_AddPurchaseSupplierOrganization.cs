using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseSupplierOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "purchase_users_id_fk_2",
                schema: "public",
                table: "purchase");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                schema: "public",
                table: "purchase",
                newName: "supplier_user_id");

            migrationBuilder.RenameIndex(
                name: "purchase_supplier_id_index",
                schema: "public",
                table: "purchase",
                newName: "purchase_supplier_user_id_index");

            migrationBuilder.AddColumn<Guid>(
                name: "supplier_organization_id",
                schema: "public",
                table: "purchase",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE public.purchase
                SET supplier_organization_id = supplier_user_id;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "supplier_organization_id",
                schema: "public",
                table: "purchase",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "purchase_supplier_organization_id_index",
                schema: "public",
                table: "purchase",
                column: "supplier_organization_id");

            migrationBuilder.AddForeignKey(
                name: "purchase_supplier_organization_id_fk",
                schema: "public",
                table: "purchase",
                column: "supplier_organization_id",
                principalSchema: "auth",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "purchase_supplier_user_id_fk",
                schema: "public",
                table: "purchase",
                column: "supplier_user_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "purchase_supplier_organization_id_fk",
                schema: "public",
                table: "purchase");

            migrationBuilder.DropForeignKey(
                name: "purchase_supplier_user_id_fk",
                schema: "public",
                table: "purchase");

            migrationBuilder.DropIndex(
                name: "purchase_supplier_organization_id_index",
                schema: "public",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "supplier_organization_id",
                schema: "public",
                table: "purchase");

            migrationBuilder.RenameColumn(
                name: "supplier_user_id",
                schema: "public",
                table: "purchase",
                newName: "supplier_id");

            migrationBuilder.RenameIndex(
                name: "purchase_supplier_user_id_index",
                schema: "public",
                table: "purchase",
                newName: "purchase_supplier_id_index");

            migrationBuilder.AddForeignKey(
                name: "purchase_users_id_fk_2",
                schema: "public",
                table: "purchase",
                column: "supplier_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

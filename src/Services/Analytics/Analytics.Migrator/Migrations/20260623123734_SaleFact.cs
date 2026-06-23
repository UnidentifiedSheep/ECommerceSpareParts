using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class SaleFact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "sale_contents_sales_fact_id_fk",
                table: "sale_contents");

            migrationBuilder.DropIndex(
                name: "analytics.entities.salesfact_who_created_idx",
                table: "sales_fact");

            migrationBuilder.DropIndex(
                name: "analytics.entities.salesfact_who_updated_idx",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "who_created",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "who_updated",
                table: "sales_fact");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "sales_fact",
                newName: "processed_at");

            migrationBuilder.AlterColumn<Guid>(
                name: "sale_id",
                table: "sale_contents",
                type: "uuid",
                maxLength: 128,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "sales_fact_created_at_index",
                table: "sales_fact",
                column: "created_at");

            migrationBuilder.AddForeignKey(
                name: "sale_contents_sales_fact_id_fk",
                table: "sale_contents",
                column: "sale_id",
                principalTable: "sales_fact",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "sale_contents_sales_fact_id_fk",
                table: "sale_contents");

            migrationBuilder.DropIndex(
                name: "sales_fact_created_at_index",
                table: "sales_fact");

            migrationBuilder.RenameColumn(
                name: "processed_at",
                table: "sales_fact",
                newName: "updated_at");

            migrationBuilder.AddColumn<Guid>(
                name: "who_created",
                table: "sales_fact",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "who_updated",
                table: "sales_fact",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "sale_id",
                table: "sale_contents",
                type: "uuid",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 128);

            migrationBuilder.CreateIndex(
                name: "analytics.entities.salesfact_who_created_idx",
                table: "sales_fact",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "analytics.entities.salesfact_who_updated_idx",
                table: "sales_fact",
                column: "who_updated");

            migrationBuilder.AddForeignKey(
                name: "sale_contents_sales_fact_id_fk",
                table: "sale_contents",
                column: "sale_id",
                principalTable: "sales_fact",
                principalColumn: "id");
        }
    }
}

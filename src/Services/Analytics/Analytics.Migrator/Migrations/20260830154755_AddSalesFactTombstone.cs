using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesFactTombstone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "sales_fact",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "sales_fact");
        }
    }
}

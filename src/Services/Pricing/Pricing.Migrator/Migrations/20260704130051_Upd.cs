using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Upd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "domain.commonentities.setting_who_updated_idx",
                table: "settings",
                newName: "pricing.entities.settings.pricingsetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.setting_who_created_idx",
                table: "settings",
                newName: "pricing.entities.settings.pricingsetting_who_created_idx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "pricing.entities.settings.pricingsetting_who_updated_idx",
                table: "settings",
                newName: "domain.commonentities.setting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.settings.pricingsetting_who_created_idx",
                table: "settings",
                newName: "domain.commonentities.setting_who_created_idx");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationDecoupling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "main.entities.organization.organizationfinancialprofile_who_updated_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "main.entities.balance.organizationfinancialprofile_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.organization.organizationfinancialprofile_who_created_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "main.entities.balance.organizationfinancialprofile_who_created_idx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "main.entities.balance.organizationfinancialprofile_who_updated_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "main.entities.organization.organizationfinancialprofile_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.balance.organizationfinancialprofile_who_created_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "main.entities.organization.organizationfinancialprofile_who_created_idx");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class TPHUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.favoritsuppliersetting_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.tmtrsuppliersetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.favoritsuppliersetting_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.tmtrsuppliersetting_who_created_idx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.tmtrsuppliersetting_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.favoritsuppliersetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.settings.supplier.tmtrsuppliersetting_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "main.entities.settings.supplier.favoritsuppliersetting_who_created_idx");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserInfoSearchColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "user_info_search_column_index",
                schema: "auth",
                table: "user_info");

            migrationBuilder.DropColumn(
                name: "search_column",
                schema: "auth",
                table: "user_info");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "search_column",
                schema: "auth",
                table: "user_info",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE auth.user_info
                SET search_column = upper(name || ' ' || surname || ' ' || coalesce(description, ''));
                """);

            migrationBuilder.CreateIndex(
                name: "user_info_search_column_index",
                schema: "auth",
                table: "user_info",
                column: "search_column")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}

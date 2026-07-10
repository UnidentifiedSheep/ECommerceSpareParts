using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCoef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_coefficients",
                schema: "public");

            migrationBuilder.DropTable(
                name: "coefficients",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coefficients",
                schema: "public",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    type = table.Column<string>(type: "character varying(56)", maxLength: 56, nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("coefficients_pk", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "product_coefficients",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    coefficient_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_coefficients_pk", x => new { x.product_id, x.coefficient_name });
                    table.ForeignKey(
                        name: "article_coefficients_coefficients_name_fk",
                        column: x => x.coefficient_name,
                        principalSchema: "public",
                        principalTable: "coefficients",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_coefficients_coefficient_name",
                schema: "public",
                table: "product_coefficients",
                column: "coefficient_name");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.productcoefficient_who_created_idx",
                schema: "public",
                table: "product_coefficients",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.productcoefficient_who_updated_idx",
                schema: "public",
                table: "product_coefficients",
                column: "who_updated");
        }
    }
}

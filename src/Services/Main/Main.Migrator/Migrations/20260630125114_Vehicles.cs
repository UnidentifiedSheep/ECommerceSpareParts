using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Vehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "user_vehicles_manufacture_index",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.DropIndex(
                name: "user_vehicles_model_index",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.DropColumn(
                name: "engine_code",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.DropColumn(
                name: "manufacture",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.DropColumn(
                name: "model",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.DropColumn(
                name: "modification",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.DropColumn(
                name: "production_year",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.AlterColumn<string>(
                name: "plate_number",
                schema: "public",
                table: "user_vehicles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                schema: "public",
                table: "user_vehicles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "vehicle_id",
                schema: "public",
                table: "user_vehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "user_vehicles_vehicle_id_index",
                schema: "public",
                table: "user_vehicles",
                column: "vehicle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "user_vehicles_vehicle_id_index",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.DropColumn(
                name: "vehicle_id",
                schema: "public",
                table: "user_vehicles");

            migrationBuilder.AlterColumn<string>(
                name: "plate_number",
                schema: "public",
                table: "user_vehicles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                schema: "public",
                table: "user_vehicles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "engine_code",
                schema: "public",
                table: "user_vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "manufacture",
                schema: "public",
                table: "user_vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "model",
                schema: "public",
                table: "user_vehicles",
                type: "character varying(125)",
                maxLength: 125,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "modification",
                schema: "public",
                table: "user_vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "production_year",
                schema: "public",
                table: "user_vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "user_vehicles_manufacture_index",
                schema: "public",
                table: "user_vehicles",
                column: "manufacture")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_model_index",
                schema: "public",
                table: "user_vehicles",
                column: "model")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}

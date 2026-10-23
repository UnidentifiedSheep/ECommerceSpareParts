using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UserToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "user_tokens_permissions_index",
                schema: "auth",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "device_id",
                schema: "auth",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "ip_address",
                schema: "auth",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "permissions",
                schema: "auth",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "user_agent",
                schema: "auth",
                table: "user_tokens");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "auth",
                table: "user_tokens",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "auth",
                table: "user_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "auth",
                table: "user_tokens");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "auth",
                table: "user_tokens",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "device_id",
                schema: "auth",
                table: "user_tokens",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<IPAddress>(
                name: "ip_address",
                schema: "auth",
                table: "user_tokens",
                type: "inet",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "permissions",
                schema: "auth",
                table: "user_tokens",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                schema: "auth",
                table: "user_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "user_tokens_permissions_index",
                schema: "auth",
                table: "user_tokens",
                column: "permissions")
                .Annotation("Npgsql:IndexMethod", "gin");
        }
    }
}

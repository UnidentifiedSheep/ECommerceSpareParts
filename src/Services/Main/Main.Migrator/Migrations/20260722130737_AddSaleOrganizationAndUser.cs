using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleOrganizationAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "sale_users_id_fk",
                schema: "public",
                table: "sale");

            migrationBuilder.RenameColumn(
                name: "buyer_id",
                schema: "public",
                table: "sale",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "sale_buyer_id_index",
                schema: "public",
                table: "sale",
                newName: "sale_user_id_index");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "auth",
                table: "users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                schema: "public",
                table: "sale",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organizations_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_balances",
                schema: "public",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_balances_pk", x => new { x.organization_id, x.currency_id });
                    table.ForeignKey(
                        name: "organization_balances_organizations_id_fk",
                        column: x => x.organization_id,
                        principalSchema: "auth",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "user_balances_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_financial_profile",
                schema: "public",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_allowed_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_financial_profile_pk", x => x.organization_id);
                    table.ForeignKey(
                        name: "organization_financial_profile_organization_id_fk",
                        column: x => x.organization_id,
                        principalSchema: "auth",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_members",
                schema: "auth",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_members_pk", x => new { x.organization_id, x.user_id });
                    table.ForeignKey(
                        name: "organization_members_organizations_id_fk",
                        column: x => x.organization_id,
                        principalSchema: "auth",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "organization_members_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AlterColumn<Guid>(
                name: "organization_id",
                schema: "public",
                table: "sale",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "user_balances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_financial_profile",
                schema: "public");

            migrationBuilder.CreateIndex(
                name: "sale_organization_id_index",
                schema: "public",
                table: "sale",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.organizationbalance_who_created_idx",
                schema: "public",
                table: "organization_balances",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.organizationbalance_who_updated_idx",
                schema: "public",
                table: "organization_balances",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "organization_balances_balance_index",
                schema: "public",
                table: "organization_balances",
                column: "balance");

            migrationBuilder.CreateIndex(
                name: "organization_balances_currency_id_index",
                schema: "public",
                table: "organization_balances",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "organization_balances_currency_id_user_id_uindex",
                schema: "public",
                table: "organization_balances",
                columns: new[] { "currency_id", "organization_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "organization_balances_user_id_index",
                schema: "public",
                table: "organization_balances",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.organization.organizationfinancialprofile_who_created_idx",
                schema: "public",
                table: "organization_financial_profile",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.organization.organizationfinancialprofile_who_updated_idx",
                schema: "public",
                table: "organization_financial_profile",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "main.entities.organization.organizationmember_who_created_idx",
                schema: "auth",
                table: "organization_members",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.organization.organizationmember_who_updated_idx",
                schema: "auth",
                table: "organization_members",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "organization_members_owner_uindex",
                schema: "auth",
                table: "organization_members",
                columns: new[] { "organization_id", "role" },
                unique: true,
                filter: "role = 'Owner'");

            migrationBuilder.CreateIndex(
                name: "organization_members_user_id_index",
                schema: "auth",
                table: "organization_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.organization.organization_who_created_idx",
                schema: "auth",
                table: "organizations",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.organization.organization_who_updated_idx",
                schema: "auth",
                table: "organizations",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "organizations_system_name_uindex",
                schema: "auth",
                table: "organizations",
                column: "system_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "sale_organizations_id_fk",
                schema: "public",
                table: "sale",
                column: "organization_id",
                principalSchema: "auth",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "sale_user_id_fk",
                schema: "public",
                table: "sale",
                column: "user_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "sale_organizations_id_fk",
                schema: "public",
                table: "sale");

            migrationBuilder.DropForeignKey(
                name: "sale_user_id_fk",
                schema: "public",
                table: "sale");

            migrationBuilder.DropTable(
                name: "organization_balances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organization_financial_profile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organization_members",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "sale_organization_id_index",
                schema: "public",
                table: "sale");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "public",
                table: "sale");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "public",
                table: "sale",
                newName: "buyer_id");

            migrationBuilder.RenameIndex(
                name: "sale_user_id_index",
                schema: "public",
                table: "sale",
                newName: "sale_buyer_id_index");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "auth",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "user_balances",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_balances_pk", x => new { x.user_id, x.currency_id });
                    table.ForeignKey(
                        name: "user_balances_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "user_balances_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_financial_profile",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    min_allowed_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_financial_profile_pk", x => x.user_id);
                    table.ForeignKey(
                        name: "user_financial_profile_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.userbalance_who_created_idx",
                schema: "public",
                table: "user_balances",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.userbalance_who_updated_idx",
                schema: "public",
                table: "user_balances",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_balances_balance_index",
                schema: "public",
                table: "user_balances",
                column: "balance");

            migrationBuilder.CreateIndex(
                name: "user_balances_currency_id_index",
                schema: "public",
                table: "user_balances",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "user_balances_currency_id_user_id_uindex",
                schema: "public",
                table: "user_balances",
                columns: new[] { "currency_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_balances_user_id_index",
                schema: "public",
                table: "user_balances",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.userfinancialprofile_who_created_idx",
                schema: "public",
                table: "user_financial_profile",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.userfinancialprofile_who_updated_idx",
                schema: "public",
                table: "user_financial_profile",
                column: "who_updated");

            migrationBuilder.AddForeignKey(
                name: "sale_users_id_fk",
                schema: "public",
                table: "sale",
                column: "buyer_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

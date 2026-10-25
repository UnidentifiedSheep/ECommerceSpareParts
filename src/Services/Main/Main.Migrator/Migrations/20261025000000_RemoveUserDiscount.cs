using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations;

public partial class RemoveUserDiscount : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "user_discounts",
			schema: "public");

		migrationBuilder.Sql("""
			DELETE FROM auth.role_permissions
			WHERE permission IN ('USERS.DISCOUNT.CREATE', 'USERS.DISCOUNT.GET');

			DELETE FROM auth.user_permissions
			WHERE permission IN ('USERS.DISCOUNT.CREATE', 'USERS.DISCOUNT.GET');

			DELETE FROM auth.permissions
			WHERE name IN ('USERS.DISCOUNT.CREATE', 'USERS.DISCOUNT.GET');
			""");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "user_discounts",
			schema: "public",
			columns: table => new
			{
				user_id = table.Column<Guid>(type: "uuid", nullable: false),
				discount = table.Column<decimal>(type: "numeric", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("user_discounts_pk", x => x.user_id);
				table.ForeignKey(
					name: "user_discounts_users_id_fk",
					column: x => x.user_id,
					principalSchema: "auth",
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.Sql("""
			INSERT INTO auth.permissions (name, created_at, updated_at)
			VALUES
				('USERS.DISCOUNT.CREATE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
				('USERS.DISCOUNT.GET', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
			ON CONFLICT (name) DO NOTHING;
			""");
	}
}

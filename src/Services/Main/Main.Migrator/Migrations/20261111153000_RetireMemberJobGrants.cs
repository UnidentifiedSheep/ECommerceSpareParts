using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations;

public partial class RetireMemberJobGrants : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
		DELETE FROM auth.role_permissions
		WHERE role = 'MEMBER' AND permission IN ('JOBS.CREATE', 'JOBS.GET');
		""");

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		// Retired privileges must not be restored on rollback.
	}
}

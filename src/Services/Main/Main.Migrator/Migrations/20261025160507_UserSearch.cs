using Main.Persistence.DatabaseObjects;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Main.Migrator.Migrations;

public partial class UserSearch : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "user_search",
			schema: "auth",
			columns: table => new
			{
				user_id = table.Column<Guid>(type: "uuid", nullable: false),
				search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("user_search_pk", x => x.user_id);
				table.ForeignKey(
					name: "user_search_users_id_fk",
					column: x => x.user_id,
					principalSchema: "auth",
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "user_phones_user_id_index", schema: "auth", table: "user_phones", column: "user_id");
		migrationBuilder.CreateIndex(
				name: "user_search_vector_index", schema: "auth", table: "user_search", column: "search_vector")
			.Annotation("Npgsql:IndexMethod", "gin");

		migrationBuilder.Sql(UserSearchSql.V1.CreateFunctions);
		migrationBuilder.Sql(UserSearchSql.V1.CreateTriggers);
		migrationBuilder.Sql(UserSearchSql.V1.Backfill);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(UserSearchSql.V1.DropTriggers);
		migrationBuilder.Sql(UserSearchSql.V1.DropFunctions);
		migrationBuilder.DropTable(name: "user_search", schema: "auth");
		migrationBuilder.DropIndex(name: "user_phones_user_id_index", schema: "auth", table: "user_phones");
	}
}

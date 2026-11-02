using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UnifyCurrencyRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_rate_history",
                schema: "public");

            // Existing rates are intentionally discarded; refresh them after deployment.
            migrationBuilder.Sql("TRUNCATE TABLE public.currency_rates");

            migrationBuilder.DropPrimaryKey(
                name: "currency_to_usd_pk",
                schema: "public",
                table: "currency_rates");

            migrationBuilder.RenameIndex(
                name: "currency_to_usd_index",
                schema: "public",
                table: "currency_rates",
                newName: "IX_currency_rates_to_currency_id");

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "public",
                table: "currency_rates",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "currency_rates_pk",
                schema: "public",
                table: "currency_rates",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "currency_rates_pair_date_idx",
                schema: "public",
                table: "currency_rates",
                columns: new[] { "from_currency_id", "to_currency_id", "created_at", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Several observations per pair cannot be restored to the old composite key.
            migrationBuilder.Sql("TRUNCATE TABLE public.currency_rates");

            migrationBuilder.DropPrimaryKey(
                name: "currency_rates_pk",
                schema: "public",
                table: "currency_rates");

            migrationBuilder.DropIndex(
                name: "currency_rates_pair_date_idx",
                schema: "public",
                table: "currency_rates");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "public",
                table: "currency_rates");

            migrationBuilder.RenameIndex(
                name: "IX_currency_rates_to_currency_id",
                schema: "public",
                table: "currency_rates",
                newName: "currency_to_usd_index");

            migrationBuilder.AddPrimaryKey(
                name: "currency_to_usd_pk",
                schema: "public",
                table: "currency_rates",
                columns: new[] { "from_currency_id", "to_currency_id" });

            migrationBuilder.CreateTable(
                name: "currency_rate_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_currency_id = table.Column<int>(type: "integer", nullable: false),
                    to_currency_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    new_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    prev_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_rate_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_currency_rate_history_currency_rates_from_currency_id_to_cu~",
                        columns: x => new { x.from_currency_id, x.to_currency_id },
                        principalSchema: "public",
                        principalTable: "currency_rates",
                        principalColumns: new[] { "from_currency_id", "to_currency_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_currency_rate_history_from_currency_id_to_currency_id",
                schema: "public",
                table: "currency_rate_history",
                columns: new[] { "from_currency_id", "to_currency_id" });

            migrationBuilder.CreateIndex(
                name: "main.entities.currency.currencyratehistory_who_created_idx",
                schema: "public",
                table: "currency_rate_history",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.currency.currencyratehistory_who_updated_idx",
                schema: "public",
                table: "currency_rate_history",
                column: "who_updated");
        }
    }
}

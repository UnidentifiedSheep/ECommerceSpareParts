using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class MakeAnalyticsRunItemRankRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH max_ranks AS (
                    SELECT run_id, COALESCE(MAX(rank), 0) AS max_rank
                    FROM public.analytics_run_items
                    GROUP BY run_id
                ),
                numbered AS (
                    SELECT item.run_id,
                           item.subject_key,
                           max_ranks.max_rank
                               + ROW_NUMBER() OVER (PARTITION BY item.run_id ORDER BY item.subject_key) AS rank
                    FROM public.analytics_run_items AS item
                    JOIN max_ranks ON max_ranks.run_id = item.run_id
                    WHERE item.rank IS NULL
                )
                UPDATE public.analytics_run_items AS item
                SET rank = numbered.rank
                FROM numbered
                WHERE item.run_id = numbered.run_id
                  AND item.subject_key = numbered.subject_key;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "rank",
                schema: "public",
                table: "analytics_run_items",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "rank",
                schema: "public",
                table: "analytics_run_items",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}

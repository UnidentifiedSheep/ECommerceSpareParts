using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesFactDailyChartFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "cost_in_base_currency",
                table: "sales_fact",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "gross_profit_in_base_currency",
                table: "sales_fact",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "sales_fact",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "products_count",
                table: "sales_fact",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "revenue_in_base_currency",
                table: "sales_fact",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE sales_fact AS sf
                SET organization_id = sf.buyer_id,
                    buyer_id = '00000000-0000-0000-0000-000000000000',
                    revenue_in_base_currency = COALESCE((
                        SELECT SUM(sc.price_in_base_currency * sc.count)
                        FROM sale_contents AS sc
                        WHERE sc.sale_id = sf.id
                    ), 0),
                    cost_in_base_currency = COALESCE((
                        SELECT SUM(scd.buy_price_in_base_currency * scd.count)
                        FROM sale_content_detail AS scd
                        INNER JOIN sale_contents AS sc ON sc.id = scd.sale_content_id
                        WHERE sc.sale_id = sf.id
                    ), 0),
                    products_count = COALESCE((
                        SELECT SUM(sc.count)
                        FROM sale_contents AS sc
                        WHERE sc.sale_id = sf.id
                    ), 0);

                UPDATE sales_fact
                SET gross_profit_in_base_currency =
                    revenue_in_base_currency - cost_in_base_currency;
                """);

            migrationBuilder.CreateIndex(
                name: "sales_fact_organization_id_index",
                table: "sales_fact",
                column: "organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "sales_fact_organization_id_index",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "cost_in_base_currency",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "gross_profit_in_base_currency",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "products_count",
                table: "sales_fact");

            migrationBuilder.DropColumn(
                name: "revenue_in_base_currency",
                table: "sales_fact");
        }
    }
}

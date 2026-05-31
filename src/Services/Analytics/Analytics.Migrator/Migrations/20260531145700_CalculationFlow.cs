using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class CalculationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "metrics_range_start_end_discriminator_u_index",
                table: "metrics");

            migrationBuilder.DropIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs");

            migrationBuilder.RenameColumn(
                name: "article_id",
                table: "sale_contents",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "sale_contents_article_id_index",
                table: "sale_contents",
                newName: "sale_contents_product_id_index");

            migrationBuilder.RenameColumn(
                name: "article_id",
                table: "purchase_contents",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "purchase_contents_article_id_index",
                table: "purchase_contents",
                newName: "purchase_contents_product_id_index");

            migrationBuilder.RenameColumn(
                name: "dimension_hash",
                table: "metrics",
                newName: "natural_key");

            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                table: "metrics",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);

            migrationBuilder.CreateIndex(
                name: "metrics_natural_key_index",
                table: "metrics",
                column: "natural_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id");

            migrationBuilder.AddForeignKey(
                name: "metric_calculation_jobs_metric_id_fk",
                table: "metric_calculation_jobs",
                column: "metric_id",
                principalTable: "metrics",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "metric_calculation_jobs_metric_id_fk",
                table: "metric_calculation_jobs");

            migrationBuilder.DropIndex(
                name: "metrics_natural_key_index",
                table: "metrics");

            migrationBuilder.DropIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "sale_contents",
                newName: "article_id");

            migrationBuilder.RenameIndex(
                name: "sale_contents_product_id_index",
                table: "sale_contents",
                newName: "sale_contents_article_id_index");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "purchase_contents",
                newName: "article_id");

            migrationBuilder.RenameIndex(
                name: "purchase_contents_product_id_index",
                table: "purchase_contents",
                newName: "purchase_contents_article_id_index");

            migrationBuilder.RenameColumn(
                name: "natural_key",
                table: "metrics",
                newName: "dimension_hash");

            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                table: "metrics",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34);

            migrationBuilder.CreateIndex(
                name: "metrics_range_start_end_discriminator_u_index",
                table: "metrics",
                columns: new[] { "discriminator", "range_start", "range_end", "dimension_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id",
                unique: true);
        }
    }
}

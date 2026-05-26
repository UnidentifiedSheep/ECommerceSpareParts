using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AfterInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "article_id",
                table: "metrics",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "analytics.entities.metrics.metric_who_updated_idx",
                table: "metrics",
                newName: "analytics.entities.metrics.productsalesmetric_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "analytics.entities.metrics.metric_who_created_idx",
                table: "metrics",
                newName: "analytics.entities.metrics.productsalesmetric_who_created_idx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "metrics",
                newName: "article_id");

            migrationBuilder.RenameIndex(
                name: "analytics.entities.metrics.productsalesmetric_who_updated_idx",
                table: "metrics",
                newName: "analytics.entities.metrics.metric_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "analytics.entities.metrics.productsalesmetric_who_created_idx",
                table: "metrics",
                newName: "analytics.entities.metrics.metric_who_created_idx");
        }
    }
}

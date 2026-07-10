using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class JobsOffersEtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "job");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "job_schedules",
                schema: "job",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    job_system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    input_state = table.Column<string>(type: "text", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    cron = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_queued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("job_schedules_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                schema: "job",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    locked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lease_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lease_holder_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    natural_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("jobs_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "price_offer_refresh_states",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    last_offers_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_offers_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_offer_refresh_state_pk", x => new { x.product_id, x.source, x.storage_name });
                });

            migrationBuilder.CreateTable(
                name: "price_offers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    offer_for_storage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    available_quantity = table.Column<int>(type: "integer", nullable: false),
                    minimum_purchase_quantity = table.Column<int>(type: "integer", nullable: false),
                    quantity_coefficient = table.Column<int>(type: "integer", nullable: false),
                    days_to_refund = table.Column<int>(type: "integer", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guaranteed_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivery_probability = table.Column<int>(type: "integer", nullable: false),
                    order_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_offer_pk", x => x.id);
                    table.UniqueConstraint("price_offer_source_source_key_uq", x => new { x.product_id, x.source, x.source_key, x.offer_for_storage });
                });

            migrationBuilder.CreateTable(
                name: "job_schedule_runs",
                schema: "job",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    queued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("job_schedule_runs_pk", x => x.id);
                    table.ForeignKey(
                        name: "job_schedule_runs_job_id_fk",
                        column: x => x.job_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "job_schedule_runs_job_schedule_id_fk",
                        column: x => x.job_schedule_id,
                        principalSchema: "job",
                        principalTable: "job_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_price_options",
                schema: "public",
                columns: table => new
                {
                    price_offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    markup = table.Column<decimal>(type: "numeric", nullable: false),
                    for_storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    delivery_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    guaranteed_delivery_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    delivery_probability = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_price_option_pk", x => x.price_offer_id);
                    table.ForeignKey(
                        name: "product_price_options_price_offer_id_fk",
                        column: x => x.price_offer_id,
                        principalSchema: "public",
                        principalTable: "price_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "job_schedule_runs_job_id_idx",
                schema: "job",
                table: "job_schedule_runs",
                column: "job_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "job_schedule_runs_job_schedule_id_scheduled_at_idx",
                schema: "job",
                table: "job_schedule_runs",
                columns: new[] { "job_schedule_id", "scheduled_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.jobschedule_who_created_idx",
                schema: "job",
                table: "job_schedules",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.jobschedule_who_updated_idx",
                schema: "job",
                table: "job_schedules",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "job_schedules_enabled_next_run_at_id_idx",
                schema: "job",
                table: "job_schedules",
                columns: new[] { "enabled", "next_run_at", "id" });

            migrationBuilder.CreateIndex(
                name: "job_schedules_job_system_name_idx",
                schema: "job",
                table: "job_schedules",
                column: "job_system_name");

            migrationBuilder.CreateIndex(
                name: "job_schedules_name_idx",
                schema: "job",
                table: "job_schedules",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.uniqjob_who_created_idx",
                schema: "job",
                table: "jobs",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.uniqjob_who_updated_idx",
                schema: "job",
                table: "jobs",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "jobs_locked_at_idx",
                schema: "job",
                table: "jobs",
                column: "locked_at");

            migrationBuilder.CreateIndex(
                name: "jobs_pending_system_name_natural_key_uq",
                schema: "job",
                table: "jobs",
                columns: new[] { "system_name", "natural_key" },
                unique: true,
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "jobs_status_id_idx",
                schema: "job",
                table: "jobs",
                columns: new[] { "status", "id" });

            migrationBuilder.CreateIndex(
                name: "jobs_system_name_idx",
                schema: "job",
                table: "jobs",
                column: "system_name");

            migrationBuilder.CreateIndex(
                name: "price_offer_currency_id_index",
                schema: "public",
                table: "price_offers",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "price_offer_expires_at_index",
                schema: "public",
                table: "price_offers",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "price_offer_offer_for_storage_index",
                schema: "public",
                table: "price_offers",
                column: "offer_for_storage");

            migrationBuilder.CreateIndex(
                name: "price_offer_product_id_index",
                schema: "public",
                table: "price_offers",
                columns: new[] { "product_id", "offer_for_storage" });

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.priceoffer_who_created_idx",
                schema: "public",
                table: "price_offers",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.priceoffer_who_updated_idx",
                schema: "public",
                table: "price_offers",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.productpriceoption_who_created_idx",
                schema: "public",
                table: "product_price_options",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.offers.productpriceoption_who_updated_idx",
                schema: "public",
                table: "product_price_options",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "product_price_option_currency_id_index",
                schema: "public",
                table: "product_price_options",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "product_price_option_score_index",
                schema: "public",
                table: "product_price_options",
                column: "score");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_schedule_runs",
                schema: "job");

            migrationBuilder.DropTable(
                name: "price_offer_refresh_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_price_options",
                schema: "public");

            migrationBuilder.DropTable(
                name: "jobs",
                schema: "job");

            migrationBuilder.DropTable(
                name: "job_schedules",
                schema: "job");

            migrationBuilder.DropTable(
                name: "price_offers",
                schema: "public");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pricing.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "msg");

            migrationBuilder.EnsureSchema(
                name: "job");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "InboxState",
                schema: "msg",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

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
                name: "markup_group",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    is_auto_generated = table.Column<bool>(type: "boolean", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("markup_group_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                schema: "msg",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "price_appliers",
                schema: "public",
                columns: table => new
                {
                    system_name = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    dsl_logic = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_appliers_pk", x => x.system_name);
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
                    delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    guaranteed_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    source_occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_probability = table.Column<int>(type: "integer", nullable: false),
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
                name: "settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("settings_pk", x => x.key);
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
                name: "markup_ranges",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    range_start = table.Column<decimal>(type: "numeric", nullable: false),
                    range_end = table.Column<decimal>(type: "numeric", nullable: false),
                    markup = table.Column<decimal>(type: "numeric", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("markup_ranges_pk", x => x.id);
                    table.ForeignKey(
                        name: "markup_ranges_markup_group_id_fk",
                        column: x => x.group_id,
                        principalTable: "markup_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "msg",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalSchema: "msg",
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalSchema: "msg",
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.CreateTable(
                name: "price_applier_states",
                schema: "public",
                columns: table => new
                {
                    price_applier_system_name = table.Column<string>(type: "text", nullable: false),
                    usage = table.Column<string>(type: "text", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("price_applier_states_pk", x => new { x.price_applier_system_name, x.usage });
                    table.ForeignKey(
                        name: "price_applier_states_price_applier_system_name_fk",
                        column: x => x.price_applier_system_name,
                        principalSchema: "public",
                        principalTable: "price_appliers",
                        principalColumn: "system_name",
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
                    markup_version = table.Column<string>(type: "text", nullable: false),
                    appliers_version = table.Column<string>(type: "text", nullable: false),
                    pricing_settings_version = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "IX_InboxState_Delivered",
                schema: "msg",
                table: "InboxState",
                column: "Delivered");

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
                name: "IX_markup_group_currency_id",
                table: "markup_group",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.markupgroup_who_created_idx",
                table: "markup_group",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.markupgroup_who_updated_idx",
                table: "markup_group",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "IX_markup_ranges_group_id",
                table: "markup_ranges",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                schema: "msg",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                schema: "msg",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                schema: "msg",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                schema: "msg",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                schema: "msg",
                table: "OutboxState",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "price_applier_states_enabled_usage_order_uq",
                schema: "public",
                table: "price_applier_states",
                columns: new[] { "usage", "order" },
                unique: true,
                filter: "enabled = true");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplierstate_who_created_idx",
                schema: "public",
                table: "price_applier_states",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplierstate_who_updated_idx",
                schema: "public",
                table: "price_applier_states",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplier_who_created_idx",
                schema: "public",
                table: "price_appliers",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.pricing.priceapplier_who_updated_idx",
                schema: "public",
                table: "price_appliers",
                column: "who_updated");

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

            migrationBuilder.CreateIndex(
                name: "pricing.entities.settings.pricingsetting_who_created_idx",
                table: "settings",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "pricing.entities.settings.pricingsetting_who_updated_idx",
                table: "settings",
                column: "who_updated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_schedule_runs",
                schema: "job");

            migrationBuilder.DropTable(
                name: "markup_ranges");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "price_applier_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_offer_refresh_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_price_options",
                schema: "public");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "jobs",
                schema: "job");

            migrationBuilder.DropTable(
                name: "job_schedules",
                schema: "job");

            migrationBuilder.DropTable(
                name: "markup_group");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "price_appliers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_offers",
                schema: "public");
        }
    }
}

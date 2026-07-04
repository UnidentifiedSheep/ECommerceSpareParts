using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Analytics.Migrator.Migrations
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

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:dblink", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

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
                name: "metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    range_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    range_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recalculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    tags = table.Column<long>(type: "bigint", nullable: false),
                    dimension_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    natural_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    depends_on = table.Column<long>(type: "bigint", nullable: false),
                    json = table.Column<string>(type: "text", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("metrics_pk", x => x.id);
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
                name: "purchases_fact",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_sum = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchases_fact_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sales_fact",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    base_currency_id = table.Column<int>(type: "integer", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_sum = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sales_fact_pk", x => x.id);
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
                name: "metric_jobs",
                columns: table => new
                {
                    metric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("metric_jobs_pk", x => new { x.metric_id, x.job_id });
                    table.ForeignKey(
                        name: "metric_jobs_job_id_fk",
                        column: x => x.job_id,
                        principalSchema: "job",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "metric_jobs_metric_id_fk",
                        column: x => x.metric_id,
                        principalTable: "metrics",
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
                name: "purchase_contents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    purchase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_contents_pk", x => x.id);
                    table.ForeignKey(
                        name: "purchase_contents_purchases_fact_id_fk",
                        column: x => x.purchase_id,
                        principalTable: "purchases_fact",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_contents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    sale_id = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    price_in_base_currency = table.Column<decimal>(type: "numeric", nullable: false),
                    discount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_contents_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_contents_sales_fact_id_fk",
                        column: x => x.sale_id,
                        principalTable: "sales_fact",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_content_detail",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    sale_content_id = table.Column<int>(type: "integer", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    buy_price = table.Column<decimal>(type: "numeric", nullable: false),
                    buy_price_in_base_currency = table.Column<decimal>(type: "numeric", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_content_detail_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_content_detail_sale_content_id_fk",
                        column: x => x.sale_content_id,
                        principalTable: "sale_contents",
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
                name: "domain.commonentities.job_who_created_idx",
                schema: "job",
                table: "jobs",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.job_who_updated_idx",
                schema: "job",
                table: "jobs",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "jobs_locked_at_idx",
                schema: "job",
                table: "jobs",
                column: "locked_at");

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
                name: "metric_jobs_job_id_idx",
                table: "metric_jobs",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "metric_jobs_metric_id_idx",
                table: "metric_jobs",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "analytics.entities.metrics.productsalesmetric_who_created_idx",
                table: "metrics",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "analytics.entities.metrics.productsalesmetric_who_updated_idx",
                table: "metrics",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "metrics_currency_id_index",
                table: "metrics",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "metrics_dirty_index",
                table: "metrics",
                column: "discriminator",
                filter: "(tags & 1) = 1");

            migrationBuilder.CreateIndex(
                name: "metrics_discriminator_article_index",
                table: "metrics",
                columns: new[] { "discriminator", "product_id" });

            migrationBuilder.CreateIndex(
                name: "metrics_natural_key_index",
                table: "metrics",
                column: "natural_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "metrics_range_depends_index",
                table: "metrics",
                columns: new[] { "depends_on", "range_start", "range_end" });

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
                name: "purchase_contents_product_id_index",
                table: "purchase_contents",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "purchase_contents_purchase_id_index",
                table: "purchase_contents",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "purchases_fact_created_at_index",
                table: "purchases_fact",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "purchases_fact_currency_id_index",
                table: "purchases_fact",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "purchases_fact_supplier_id_index",
                table: "purchases_fact",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_detail_currency_id_index",
                table: "sale_content_detail",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_detail_sale_content_id_index",
                table: "sale_content_detail",
                column: "sale_content_id");

            migrationBuilder.CreateIndex(
                name: "sale_contents_product_id_index",
                table: "sale_contents",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "sale_contents_sale_id_index",
                table: "sale_contents",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "sales_fact_buyer_id_index",
                table: "sales_fact",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "sales_fact_created_at_index",
                table: "sales_fact",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "sales_fact_currency_id_index",
                table: "sales_fact",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.setting_who_created_idx",
                table: "settings",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.setting_who_updated_idx",
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
                name: "metric_jobs");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "purchase_contents");

            migrationBuilder.DropTable(
                name: "sale_content_detail");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "job_schedules",
                schema: "job");

            migrationBuilder.DropTable(
                name: "jobs",
                schema: "job");

            migrationBuilder.DropTable(
                name: "metrics");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "purchases_fact");

            migrationBuilder.DropTable(
                name: "sale_contents");

            migrationBuilder.DropTable(
                name: "sales_fact");
        }
    }
}

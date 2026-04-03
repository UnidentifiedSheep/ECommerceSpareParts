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

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    to_usd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currencies_pk", x => x.id);
                });

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
                name: "metric_calculation_jobs",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metric_system_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    error_message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("request_id_pk", x => x.request_id);
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
                name: "metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    range_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    range_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recalculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    tags = table.Column<long>(type: "bigint", nullable: false),
                    dimension_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dimension_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    depends_on = table.Column<long>(type: "bigint", nullable: false),
                    json = table.Column<string>(type: "text", nullable: true),
                    article_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("metrics_pk", x => x.id);
                    table.ForeignKey(
                        name: "metrics_currencies_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchases_fact",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_sum = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchases_fact_pk", x => x.id);
                    table.ForeignKey(
                        name: "purchases_fact_currencies_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_content_detail",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    buy_price = table.Column<decimal>(type: "numeric", nullable: true),
                    count = table.Column<int>(type: "integer", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_content_detail_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_content_detail_currencies_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_fact",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_sum = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sales_fact_pk", x => x.id);
                    table.ForeignKey(
                        name: "sales_fact_currencies_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                    purchase_id = table.Column<string>(type: "text", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
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
                    sale_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    discount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_contents_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_contents_sales_fact_id_fk",
                        column: x => x.sale_id,
                        principalTable: "sales_fact",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                schema: "msg",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_created_at_index",
                table: "metric_calculation_jobs",
                column: "create_at");

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_metric_id_index",
                table: "metric_calculation_jobs",
                column: "metric_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "metrics_calc_jobs_status_name_index",
                table: "metric_calculation_jobs",
                columns: new[] { "status", "metric_system_name" });

            migrationBuilder.CreateIndex(
                name: "metrics_created_at_index",
                table: "metrics",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "metrics_created_by_index",
                table: "metrics",
                column: "created_by");

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
                columns: new[] { "discriminator", "article_id" });

            migrationBuilder.CreateIndex(
                name: "metrics_range_depends_index",
                table: "metrics",
                columns: new[] { "depends_on", "range_start", "range_end" });

            migrationBuilder.CreateIndex(
                name: "metrics_range_start_end_discriminator_u_index",
                table: "metrics",
                columns: new[] { "discriminator", "range_start", "range_end", "dimension_hash" },
                unique: true);

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
                name: "purchase_contents_article_id_index",
                table: "purchase_contents",
                column: "article_id");

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
                name: "sale_contents_article_id_index",
                table: "sale_contents",
                column: "article_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metric_calculation_jobs");

            migrationBuilder.DropTable(
                name: "metrics");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "purchase_contents");

            migrationBuilder.DropTable(
                name: "sale_content_detail");

            migrationBuilder.DropTable(
                name: "sale_contents");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "purchases_fact");

            migrationBuilder.DropTable(
                name: "sales_fact");

            migrationBuilder.DropTable(
                name: "currencies");
        }
    }
}

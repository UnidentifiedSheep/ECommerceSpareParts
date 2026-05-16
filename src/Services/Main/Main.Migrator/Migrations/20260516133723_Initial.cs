using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "msg");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:dblink", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("categories_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "coefficients",
                schema: "public",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false),
                    type = table.Column<string>(type: "character varying(56)", maxLength: 56, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("coefficients_pk", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    short_name = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    currency_sign = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    code = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currency_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("events_id_pk", x => x.id);
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
                name: "permissions",
                schema: "auth",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("permissions_pk", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "producer",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    image_path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("producer_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "auth",
                columns: table => new
                {
                    normalized_name = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.normalized_name);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                schema: "public",
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
                name: "storages",
                schema: "public",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storages_pk", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    user_name = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_coefficients",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    coefficient_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valid_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_coefficients_pk", x => new { x.product_id, x.coefficient_name });
                    table.ForeignKey(
                        name: "article_coefficients_coefficients_name_fk",
                        column: x => x.coefficient_name,
                        principalSchema: "public",
                        principalTable: "coefficients",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "currency_rates",
                schema: "public",
                columns: table => new
                {
                    from_currency_id = table.Column<int>(type: "integer", nullable: false),
                    to_currency_id = table.Column<int>(type: "integer", nullable: false),
                    rate = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currency_to_usd_pk", x => new { x.from_currency_id, x.to_currency_id });
                    table.ForeignKey(
                        name: "FK_currency_rates_currency_from_currency_id",
                        column: x => x.from_currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_currency_rates_currency_to_currency_id",
                        column: x => x.to_currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
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
                name: "producers_other_names",
                schema: "public",
                columns: table => new
                {
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    other_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    where_used = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("producers_other_names_pk", x => new { x.producer_id, x.other_name, x.where_used });
                    table.ForeignKey(
                        name: "producers_other_names_producer_id_fk",
                        column: x => x.producer_id,
                        principalSchema: "public",
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PairId = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    packing_unit = table.Column<int>(type: "integer", nullable: true),
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    indicator = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    popularity = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    normalized_sku = table.Column<string>(type: "text", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("products_id_pk", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_products_PairId",
                        column: x => x.PairId,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "producer_id_fk",
                        column: x => x.producer_id,
                        principalSchema: "public",
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "products_categories_id_fk",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "auth",
                columns: table => new
                {
                    role = table.Column<string>(type: "character varying(24)", nullable: false),
                    permission = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_permissions_pk", x => new { x.role, x.permission });
                    table.ForeignKey(
                        name: "role_permissions_permissions_name_fk",
                        column: x => x.permission,
                        principalSchema: "auth",
                        principalTable: "permissions",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "role_permissions_roles_id_fk",
                        column: x => x.role,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "normalized_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    buyer_approved = table.Column<bool>(type: "boolean", nullable: false),
                    seller_approved = table.Column<bool>(type: "boolean", nullable: false),
                    signed_total_price = table.Column<string>(type: "text", nullable: false),
                    is_canceled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("orders_pk", x => x.id);
                    table.ForeignKey(
                        name: "orders_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "orders_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_owners",
                schema: "public",
                columns: table => new
                {
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_owners_pk", x => new { x.storage_name, x.user_id });
                    table.ForeignKey(
                        name: "storage_owners_storages_name_fk",
                        column: x => x.storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_owners_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_routes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    from_storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    to_storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    distance_m = table.Column<int>(type: "integer", nullable: false),
                    route_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    pricing_model = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    delivery_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    price_kg = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_m3 = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_order = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    minimum_price = table.Column<decimal>(type: "numeric", nullable: false),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_routes_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_routes_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_routes_storages_name_fk",
                        column: x => x.from_storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_routes_storages_name_fk_2",
                        column: x => x.to_storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_routes_users_id_fk",
                        column: x => x.carrier_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    type = table.Column<string>(type: "character varying(28)", maxLength: 28, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    transaction_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reversed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reversed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("transactions_pk", x => x.id);
                    table.ForeignKey(
                        name: "transactions_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transactions_users_id_fk",
                        column: x => x.sender_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transactions_users_id_fk_2",
                        column: x => x.receiver_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transactions_users_id_fk_4",
                        column: x => x.reversed_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_balances",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_balances_pk", x => new { x.user_id, x.currency_id });
                    table.ForeignKey(
                        name: "user_balances_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "user_balances_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_discounts",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_discounts_pk", x => x.user_id);
                    table.ForeignKey(
                        name: "user_discounts_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_emails",
                schema: "auth",
                columns: table => new
                {
                    normalized_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    email_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_emails_primary_key", x => x.normalized_email);
                    table.ForeignKey(
                        name: "user_emails_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_info",
                schema: "auth",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    surname = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    search_column = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_info_pk", x => x.user_id);
                    table.ForeignKey(
                        name: "user_info_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_permissions",
                schema: "auth",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_permissions_pk", x => new { x.user_id, x.permission });
                    table.ForeignKey(
                        name: "user_permissions_permissions_name_fk",
                        column: x => x.permission,
                        principalSchema: "auth",
                        principalTable: "permissions",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "user_permissions_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_phones",
                schema: "auth",
                columns: table => new
                {
                    normalized_phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    phone_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_phones_pk", x => x.normalized_phone);
                    table.ForeignKey(
                        name: "user_phones_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "auth",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_name = table.Column<string>(type: "character varying(24)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_roles_pk", x => new { x.user_id, x.role_name });
                    table.ForeignKey(
                        name: "user_roles_roles_name_fk",
                        column: x => x.role_name,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "normalized_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_roles_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_search_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_place = table.Column<string>(type: "text", nullable: false),
                    query = table.Column<string>(type: "jsonb", nullable: false),
                    search_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_search_history_pk", x => x.id);
                    table.ForeignKey(
                        name: "user_search_history_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    permissions = table.Column<List<string>>(type: "text[]", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoke_reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_tokens_pk", x => x.id);
                    table.ForeignKey(
                        name: "user_tokens_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_vehicles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    plate_number = table.Column<string>(type: "text", nullable: false),
                    manufacture = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    model = table.Column<string>(type: "character varying(125)", maxLength: 125, nullable: false),
                    modification = table.Column<string>(type: "text", nullable: true),
                    engine_code = table.Column<string>(type: "text", nullable: true),
                    production_year = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_vehicles_pk", x => x.id);
                    table.ForeignKey(
                        name: "user_vehicles_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "currency_rate_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_currency_id = table.Column<int>(type: "integer", nullable: false),
                    to_currency_id = table.Column<int>(type: "integer", nullable: false),
                    prev_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    new_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "cart",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cart_pk", x => new { x.user_id, x.product_id });
                    table.ForeignKey(
                        name: "cart_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "cart_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_characteristics",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_characteristics_pk", x => new { x.product_id, x.name });
                    table.ForeignKey(
                        name: "product_characteristics_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_contents",
                schema: "public",
                columns: table => new
                {
                    parent_product_id = table.Column<int>(type: "integer", nullable: false),
                    child_product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_contents_pk", x => new { x.parent_product_id, x.child_product_id });
                    table.ForeignKey(
                        name: "product_contents_child_fk",
                        column: x => x.child_product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "product_contents_parent_fk",
                        column: x => x.parent_product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_crosses",
                schema: "public",
                columns: table => new
                {
                    left_product_id = table.Column<int>(type: "integer", nullable: false),
                    right_product_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_crosses_pk", x => new { x.left_product_id, x.right_product_id });
                    table.ForeignKey(
                        name: "FK_product_crosses_products_left_product_id",
                        column: x => x.left_product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_crosses_products_right_product_id",
                        column: x => x.right_product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_eans",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    ean = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_eans_pk", x => new { x.product_id, x.ean });
                    table.ForeignKey(
                        name: "product_eans_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_images_pk", x => new { x.product_id, x.path });
                    table.ForeignKey(
                        name: "product_images_product_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_sizes",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    length = table.Column<decimal>(type: "numeric", nullable: false),
                    width = table.Column<decimal>(type: "numeric", nullable: false),
                    height = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    volume_m3 = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_sizes_pk", x => x.product_id);
                    table.ForeignKey(
                        name: "product_sizes_products_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_weights",
                schema: "public",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_weights_pk", x => x.product_id);
                    table.ForeignKey(
                        name: "product_weight_products_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_content",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    buy_price = table.Column<decimal>(type: "numeric", nullable: false),
                    buy_price_in_base_currency = table.Column<decimal>(type: "numeric", nullable: false),
                    base_currency_id = table.Column<int>(type: "integer", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    purchase_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_content_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_content_base_currency_id_fk",
                        column: x => x.base_currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_products_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_storages_name_fk",
                        column: x => x.storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "storage_content_reservations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    reserved_count = table.Column<int>(type: "integer", nullable: false),
                    current_count = table.Column<int>(type: "integer", nullable: false),
                    proposed_price = table.Column<decimal>(type: "numeric", nullable: true),
                    proposed_currency_id = table.Column<int>(type: "integer", nullable: true),
                    is_done = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_content_reservations_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_content_reservations_currency_id_fk",
                        column: x => x.proposed_currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_content_reservations_products_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_reservations_users_id_fk",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_content_reservations_users_id_fk_2",
                        column: x => x.who_updated,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_content_reservations_users_id_fk_3",
                        column: x => x.who_created,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    signed_price = table.Column<string>(type: "text", nullable: false),
                    locked_price = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_items_pk", x => x.id);
                    table.ForeignKey(
                        name: "order_items_articles_id_fk",
                        column: x => x.article_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_items_orders_id_fk",
                        column: x => x.order_id,
                        principalSchema: "public",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    purchase_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    state = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_pk", x => x.id);
                    table.ForeignKey(
                        name: "purchase_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_storages_name_fk",
                        column: x => x.storage,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_transactions_id_fk",
                        column: x => x.transaction_id,
                        principalSchema: "public",
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_users_id_fk_2",
                        column: x => x.supplier_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    sale_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    who_created = table.Column<Guid>(type: "uuid", nullable: true),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_storages_name_fk",
                        column: x => x.storage_name,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_transactions_id_fk",
                        column: x => x.transaction_id,
                        principalSchema: "public",
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_users_id_fk",
                        column: x => x.buyer_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_content",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    storage_content_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    total_sum = table.Column<decimal>(type: "numeric", nullable: false),
                    comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_content_pk", x => x.id);
                    table.ForeignKey(
                        name: "purchase_content_products_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_content_purchase_id_fk",
                        column: x => x.purchase_id,
                        principalSchema: "public",
                        principalTable: "purchase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "purchase_content_storage_content_id_fk",
                        column: x => x.storage_content_id,
                        principalSchema: "public",
                        principalTable: "storage_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "purchase_logistics",
                schema: "public",
                columns: table => new
                {
                    purchase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pricing_model = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    route_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    price_kg = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_m3 = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_order = table.Column<decimal>(type: "numeric", nullable: false),
                    minimum_price = table.Column<decimal>(type: "numeric", nullable: true),
                    minimum_price_applied = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_logistics_pk", x => x.purchase_id);
                    table.ForeignKey(
                        name: "purchase_logistics_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_logistics_purchase_id_fk",
                        column: x => x.purchase_id,
                        principalSchema: "public",
                        principalTable: "purchase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "purchase_logistics_storage_routes_id_fk",
                        column: x => x.route_id,
                        principalSchema: "public",
                        principalTable: "storage_routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_logistics_transactions_id_fk",
                        column: x => x.transaction_id,
                        principalSchema: "public",
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_content",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    total_sum = table.Column<decimal>(type: "numeric", nullable: false),
                    comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    discount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_content_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_content_products_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_content_sale_id_fk",
                        column: x => x.sale_id,
                        principalSchema: "public",
                        principalTable: "sale",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_content_logistics",
                schema: "public",
                columns: table => new
                {
                    purchase_content_id = table.Column<int>(type: "integer", nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric", nullable: false),
                    area_m3 = table.Column<decimal>(type: "numeric", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_content_logistics_pk", x => x.purchase_content_id);
                    table.ForeignKey(
                        name: "purchase_content_logistics_purchase_content_id_fk",
                        column: x => x.purchase_content_id,
                        principalSchema: "public",
                        principalTable: "purchase_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_content_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_content_id = table.Column<int>(type: "integer", nullable: false),
                    storage_content_id = table.Column<int>(type: "integer", nullable: false),
                    storage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    buy_price = table.Column<decimal>(type: "numeric", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    purchase_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_content_details_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_content_details_currency_id_fk",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_content_details_sale_content_id_fk",
                        column: x => x.sale_content_id,
                        principalSchema: "public",
                        principalTable: "sale_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "sale_content_details_storage_content_id_fk",
                        column: x => x.storage_content_id,
                        principalSchema: "public",
                        principalTable: "storage_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "sale_content_details_storages_name_fk",
                        column: x => x.storage,
                        principalSchema: "public",
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "cart_product_id_idx",
                schema: "public",
                table: "cart",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.cart.cart_who_created_idx",
                schema: "public",
                table: "cart",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.cart.cart_who_updated_idx",
                schema: "public",
                table: "cart",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "categories_name_index",
                schema: "public",
                table: "categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "currency_code_uindex",
                schema: "public",
                table: "currency",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_currency_sign_uindex",
                schema: "public",
                table: "currency",
                column: "currency_sign",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_name_uindex",
                schema: "public",
                table: "currency",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_short_name_uindex",
                schema: "public",
                table: "currency",
                column: "short_name",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "currency_to_usd_index",
                schema: "public",
                table: "currency_rates",
                column: "to_currency_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.currency.currencyrate_who_created_idx",
                schema: "public",
                table: "currency_rates",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.currency.currencyrate_who_updated_idx",
                schema: "public",
                table: "currency_rates",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "event_discriminator_idx",
                schema: "public",
                table: "events",
                column: "discriminator");

            migrationBuilder.CreateIndex(
                name: "main.entities.event.storagemovementevent_who_created_idx",
                schema: "public",
                table: "events",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.event.storagemovementevent_who_updated_idx",
                schema: "public",
                table: "events",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                schema: "msg",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "order_items_order_id_index",
                schema: "public",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "order_items_product_id_index",
                schema: "public",
                table: "order_items",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.order.order_who_created_idx",
                schema: "public",
                table: "orders",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.order.order_who_updated_idx",
                schema: "public",
                table: "orders",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "orders_buyer_approved_index",
                schema: "public",
                table: "orders",
                column: "buyer_approved");

            migrationBuilder.CreateIndex(
                name: "orders_currency_id_index",
                schema: "public",
                table: "orders",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "orders_is_canceled_index",
                schema: "public",
                table: "orders",
                column: "is_canceled");

            migrationBuilder.CreateIndex(
                name: "orders_seller_approved_index",
                schema: "public",
                table: "orders",
                column: "seller_approved");

            migrationBuilder.CreateIndex(
                name: "orders_status_index",
                schema: "public",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "orders_user_id_is_canceled_index",
                schema: "public",
                table: "orders",
                columns: new[] { "user_id", "is_canceled" });

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
                name: "main.entities.auth.permission_who_created_idx",
                schema: "auth",
                table: "permissions",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.permission_who_updated_idx",
                schema: "auth",
                table: "permissions",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "main.entities.producer.producer_who_created_idx",
                schema: "public",
                table: "producer",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.producer.producer_who_updated_idx",
                schema: "public",
                table: "producer",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "producer_name_uindex",
                schema: "public",
                table: "producer",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_id_index",
                schema: "public",
                table: "producers_other_names",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_other_name_index",
                schema: "public",
                table: "producers_other_names",
                column: "other_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "producers_other_names_where_used_index",
                schema: "public",
                table: "producers_other_names",
                column: "where_used")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "product_characteristics_id_index",
                schema: "public",
                table: "product_characteristics",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "product_characteristics_name_value_index",
                schema: "public",
                table: "product_characteristics",
                columns: new[] { "name", "value" });

            migrationBuilder.CreateIndex(
                name: "IX_product_coefficients_coefficient_name",
                schema: "public",
                table: "product_coefficients",
                column: "coefficient_name");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.productcoefficient_who_created_idx",
                schema: "public",
                table: "product_coefficients",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.productcoefficient_who_updated_idx",
                schema: "public",
                table: "product_coefficients",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "product_contents_child_id_idx",
                schema: "public",
                table: "product_contents",
                column: "child_product_id");

            migrationBuilder.CreateIndex(
                name: "product_crosses_right_id_idx",
                schema: "public",
                table: "product_crosses",
                column: "right_product_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.product_who_created_idx",
                schema: "public",
                table: "products",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.product.product_who_updated_idx",
                schema: "public",
                table: "products",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "products_category_id_index",
                schema: "public",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "products_pair_id_index",
                schema: "public",
                table: "products",
                column: "PairId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "products_popularity_index",
                schema: "public",
                table: "products",
                column: "popularity");

            migrationBuilder.CreateIndex(
                name: "products_producer_id_index",
                schema: "public",
                table: "products",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.purchase.purchase_who_created_idx",
                schema: "public",
                table: "purchase",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.purchase.purchase_who_updated_idx",
                schema: "public",
                table: "purchase",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "purchase_comment_index",
                schema: "public",
                table: "purchase",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "purchase_currency_id_index",
                schema: "public",
                table: "purchase",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "purchase_purchase_datetime_index",
                schema: "public",
                table: "purchase",
                column: "purchase_datetime");

            migrationBuilder.CreateIndex(
                name: "purchase_state_index",
                schema: "public",
                table: "purchase",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "purchase_storage_index",
                schema: "public",
                table: "purchase",
                column: "storage");

            migrationBuilder.CreateIndex(
                name: "purchase_supplier_id_index",
                schema: "public",
                table: "purchase",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "purchase_transaction_id_index",
                schema: "public",
                table: "purchase",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "purchase_content_comment_index",
                schema: "public",
                table: "purchase_content",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "purchase_content_product_id_index",
                schema: "public",
                table: "purchase_content",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "purchase_content_purchase_id_index",
                schema: "public",
                table: "purchase_content",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "purchase_content_storage_content_id_uindex",
                schema: "public",
                table: "purchase_content",
                column: "storage_content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_logistics_currency_id",
                schema: "public",
                table: "purchase_logistics",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_logistics_route_id",
                schema: "public",
                table: "purchase_logistics",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "purchase_logistics_transaction_id_uindex",
                schema: "public",
                table: "purchase_logistics",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_name",
                schema: "auth",
                table: "role_permissions",
                column: "permission");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.role_who_created_idx",
                schema: "auth",
                table: "roles",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.role_who_updated_idx",
                schema: "auth",
                table: "roles",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "main.entities.sale.sale_who_created_idx",
                schema: "public",
                table: "sale",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.sale.sale_who_updated_idx",
                schema: "public",
                table: "sale",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "sale_buyer_id_index",
                schema: "public",
                table: "sale",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "sale_comment_index",
                schema: "public",
                table: "sale",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "sale_currency_id_index",
                schema: "public",
                table: "sale",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "sale_sale_datetime_index",
                schema: "public",
                table: "sale",
                column: "sale_datetime");

            migrationBuilder.CreateIndex(
                name: "sale_state_index",
                schema: "public",
                table: "sale",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "sale_storage_name_index",
                schema: "public",
                table: "sale",
                column: "storage_name");

            migrationBuilder.CreateIndex(
                name: "sale_transaction_id_index",
                schema: "public",
                table: "sale",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_comment_index",
                schema: "public",
                table: "sale_content",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "sale_content_product_id_index",
                schema: "public",
                table: "sale_content",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_sale_id_index",
                schema: "public",
                table: "sale_content",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_currency_id_index",
                schema: "public",
                table: "sale_content_details",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_sale_content_id_index",
                schema: "public",
                table: "sale_content_details",
                column: "sale_content_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_storage_content_id_index",
                schema: "public",
                table: "sale_content_details",
                column: "storage_content_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_storage_index",
                schema: "public",
                table: "sale_content_details",
                column: "storage");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.setting_who_created_idx",
                schema: "public",
                table: "settings",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "domain.commonentities.setting_who_updated_idx",
                schema: "public",
                table: "settings",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "IX_storage_content_base_currency_id",
                schema: "public",
                table: "storage_content",
                column: "base_currency_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storagecontent_who_created_idx",
                schema: "public",
                table: "storage_content",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storagecontent_who_updated_idx",
                schema: "public",
                table: "storage_content",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_content_buy_price_index",
                schema: "public",
                table: "storage_content",
                column: "buy_price");

            migrationBuilder.CreateIndex(
                name: "storage_content_currency_id_index",
                schema: "public",
                table: "storage_content",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "storage_content_product_id_count_index",
                schema: "public",
                table: "storage_content",
                columns: new[] { "product_id", "count" });

            migrationBuilder.CreateIndex(
                name: "storage_content_product_id_storage_name_index",
                schema: "public",
                table: "storage_content",
                columns: new[] { "product_id", "storage_name" });

            migrationBuilder.CreateIndex(
                name: "storage_content_purchase_datetime_index",
                schema: "public",
                table: "storage_content",
                column: "purchase_datetime");

            migrationBuilder.CreateIndex(
                name: "storage_content_storage_name_index",
                schema: "public",
                table: "storage_content",
                column: "storage_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "storage_content_storage_name_product_id_index",
                schema: "public",
                table: "storage_content",
                columns: new[] { "storage_name", "product_id" });

            migrationBuilder.CreateIndex(
                name: "IX_storage_content_reservations_proposed_currency_id",
                schema: "public",
                table: "storage_content_reservations",
                column: "proposed_currency_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storagecontentreservation_who_created_idx",
                schema: "public",
                table: "storage_content_reservations",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storagecontentreservation_who_updated_idx",
                schema: "public",
                table: "storage_content_reservations",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_comment_index",
                schema: "public",
                table: "storage_content_reservations",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_is_done_index",
                schema: "public",
                table: "storage_content_reservations",
                column: "is_done");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_product_id_is_done_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "product_id", "is_done" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_product_id_is_locked_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "product_id", "is_locked" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_user_id_is_done_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "user_id", "is_done" });

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageowner_who_created_idx",
                schema: "public",
                table: "storage_owners",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageowner_who_updated_idx",
                schema: "public",
                table: "storage_owners",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_owners_owner_id_index",
                schema: "public",
                table: "storage_owners",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_storage_routes_to_storage_name",
                schema: "public",
                table: "storage_routes",
                column: "to_storage_name");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageroute_who_created_idx",
                schema: "public",
                table: "storage_routes",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storageroute_who_updated_idx",
                schema: "public",
                table: "storage_routes",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_from_to_active_uindex",
                schema: "public",
                table: "storage_routes",
                columns: new[] { "from_storage_name", "to_storage_name", "is_active" },
                unique: true,
                filter: "(is_active = true)");

            migrationBuilder.CreateIndex(
                name: "storage_routes_carrier_id_index",
                schema: "public",
                table: "storage_routes",
                column: "carrier_id");

            migrationBuilder.CreateIndex(
                name: "storage_routes_currency_id_index",
                schema: "public",
                table: "storage_routes",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storage_who_created_idx",
                schema: "public",
                table: "storages",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.storage_who_updated_idx",
                schema: "public",
                table: "storages",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storages_description_index",
                schema: "public",
                table: "storages",
                column: "description")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "storages_location_index",
                schema: "public",
                table: "storages",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "storages_type_index",
                schema: "public",
                table: "storages",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_currency_id",
                schema: "public",
                table: "transactions",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.transaction_who_created_idx",
                schema: "public",
                table: "transactions",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.transaction_who_updated_idx",
                schema: "public",
                table: "transactions",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "transactions_deleted_by_index",
                schema: "public",
                table: "transactions",
                column: "reversed_by");

            migrationBuilder.CreateIndex(
                name: "transactions_receiver_id_index",
                schema: "public",
                table: "transactions",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "transactions_sender_id_receiver_id_index",
                schema: "public",
                table: "transactions",
                columns: new[] { "sender_id", "receiver_id" });

            migrationBuilder.CreateIndex(
                name: "transactions_transaction_datetime_id_index",
                schema: "public",
                table: "transactions",
                columns: new[] { "transaction_datetime", "id" });

            migrationBuilder.CreateIndex(
                name: "transactions_transaction_datetime_sender_id_receiver_id_idx",
                schema: "public",
                table: "transactions",
                column: "transaction_datetime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "transactions_type_index",
                schema: "public",
                table: "transactions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.userbalance_who_created_idx",
                schema: "public",
                table: "user_balances",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.balance.userbalance_who_updated_idx",
                schema: "public",
                table: "user_balances",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_balances_balance_index",
                schema: "public",
                table: "user_balances",
                column: "balance");

            migrationBuilder.CreateIndex(
                name: "user_balances_currency_id_index",
                schema: "public",
                table: "user_balances",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "user_balances_currency_id_user_id_uindex",
                schema: "public",
                table: "user_balances",
                columns: new[] { "currency_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_balances_user_id_index",
                schema: "public",
                table: "user_balances",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.useremail_who_created_idx",
                schema: "auth",
                table: "user_emails",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.useremail_who_updated_idx",
                schema: "auth",
                table: "user_emails",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_emails_normalized_email_index",
                schema: "auth",
                table: "user_emails",
                column: "normalized_email")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_emails_user_id_index",
                schema: "auth",
                table: "user_emails",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_emails_user_id_is_primary_uindex",
                schema: "auth",
                table: "user_emails",
                columns: new[] { "user_id", "is_primary" },
                unique: true,
                filter: "(is_primary = true)");

            migrationBuilder.CreateIndex(
                name: "user_info_description_index",
                schema: "auth",
                table: "user_info",
                column: "description")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_info_name_index",
                schema: "auth",
                table: "user_info",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_info_search_column_index",
                schema: "auth",
                table: "user_info",
                column: "search_column")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_info_surname_index",
                schema: "auth",
                table: "user_info",
                column: "surname")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_user_permissions_permission",
                schema: "auth",
                table: "user_permissions",
                column: "permission");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.userpermission_who_created_idx",
                schema: "auth",
                table: "user_permissions",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.userpermission_who_updated_idx",
                schema: "auth",
                table: "user_permissions",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.userphone_who_created_idx",
                schema: "auth",
                table: "user_phones",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.userphone_who_updated_idx",
                schema: "auth",
                table: "user_phones",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_phones_normalized_phone_index",
                schema: "auth",
                table: "user_phones",
                column: "normalized_phone")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_phones_user_id_is_primary_uindex",
                schema: "auth",
                table: "user_phones",
                columns: new[] { "user_id", "is_primary" },
                unique: true,
                filter: "(is_primary = true)");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                schema: "auth",
                table: "user_roles",
                column: "role_name");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.userrole_who_created_idx",
                schema: "auth",
                table: "user_roles",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.userrole_who_updated_idx",
                schema: "auth",
                table: "user_roles",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_search_history_search_date_time_index",
                schema: "public",
                table: "user_search_history",
                column: "search_date_time");

            migrationBuilder.CreateIndex(
                name: "user_search_history_search_place_index",
                schema: "public",
                table: "user_search_history",
                column: "search_place");

            migrationBuilder.CreateIndex(
                name: "user_search_history_user_id_search_place_index",
                schema: "public",
                table: "user_search_history",
                columns: new[] { "user_id", "search_place" });

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.usertoken_who_created_idx",
                schema: "auth",
                table: "user_tokens",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.auth.usertoken_who_updated_idx",
                schema: "auth",
                table: "user_tokens",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_tokens_expires_at_index",
                schema: "auth",
                table: "user_tokens",
                column: "expires_at",
                filter: "((revoked = false) AND (expires_at IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "user_tokens_permissions_index",
                schema: "auth",
                table: "user_tokens",
                column: "permissions")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "user_tokens_token_hash_uindex",
                schema: "auth",
                table: "user_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_tokens_user_id_index",
                schema: "auth",
                table: "user_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.uservehicle_who_created_idx",
                schema: "public",
                table: "user_vehicles",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.uservehicle_who_updated_idx",
                schema: "public",
                table: "user_vehicles",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "user_vehicles_comment_index",
                schema: "public",
                table: "user_vehicles",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_manufacture_index",
                schema: "public",
                table: "user_vehicles",
                column: "manufacture")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_model_index",
                schema: "public",
                table: "user_vehicles",
                column: "model")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_plate_number_uindex",
                schema: "public",
                table: "user_vehicles",
                column: "plate_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_vehicles_user_id_index",
                schema: "public",
                table: "user_vehicles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_vehicles_vin_uindex",
                schema: "public",
                table: "user_vehicles",
                column: "vin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "main.entities.user.user_who_created_idx",
                schema: "auth",
                table: "users",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.user.user_who_updated_idx",
                schema: "auth",
                table: "users",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "products_sku_index",
                schema: "public",
                table: "products",
                column: "normalized_sku")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" })
                .Annotation("Relational:ColumnName", "normalized_sku");

            migrationBuilder.CreateIndex(
                name: "products_stock_index",
                schema: "public",
                table: "products",
                column: "stock")
                .Annotation("Relational:ColumnName", "stock");

            migrationBuilder.CreateIndex(
                name: "products_normalized_sku_producer_id_index",
                schema: "public",
                table: "products",
                columns: new[] { "normalized_sku", "producer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_normalized_user_name_index",
                schema: "auth",
                table: "users",
                column: "normalized_user_name")
                .Annotation("MaxLength", 36)
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" })
                .Annotation("Relational:ColumnName", "normalized_user_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart",
                schema: "public");

            migrationBuilder.DropTable(
                name: "currency_rate_history",
                schema: "public");

            migrationBuilder.DropTable(
                name: "events",
                schema: "public");

            migrationBuilder.DropTable(
                name: "order_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "producers_other_names",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_characteristics",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_coefficients",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_contents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_crosses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_eans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_images",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_sizes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_weights",
                schema: "public");

            migrationBuilder.DropTable(
                name: "purchase_content_logistics",
                schema: "public");

            migrationBuilder.DropTable(
                name: "purchase_logistics",
                schema: "public");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "sale_content_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage_content_reservations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage_owners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_balances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_discounts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_emails",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_info",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_phones",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_search_history",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_vehicles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "currency_rates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "coefficients",
                schema: "public");

            migrationBuilder.DropTable(
                name: "purchase_content",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage_routes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sale_content",
                schema: "public");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "purchase",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage_content",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sale",
                schema: "public");

            migrationBuilder.DropTable(
                name: "products",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "producer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "currency",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");
        }
    }
}

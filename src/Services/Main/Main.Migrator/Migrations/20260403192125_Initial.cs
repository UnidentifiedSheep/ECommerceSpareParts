using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

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
                name: "msg");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:car_types", "PassengerCar,CommercialVehicle,Motorbike")
                .Annotation("Npgsql:PostgresExtension:dblink", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateSequence<int>(
                name: "storage_movement_id_seq");

            migrationBuilder.CreateSequence<int>(
                name: "table_name_id_seq");

            migrationBuilder.CreateTable(
                name: "categories",
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
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false),
                    type = table.Column<string>(type: "character varying(56)", maxLength: 56, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("coefficients_pk", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "currency",
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
                name: "default_settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("default_settings_pk", x => x.key);
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("permissions_pk", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "producer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_oe = table.Column<bool>(type: "boolean", nullable: false),
                    image_path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "storages",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
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
                    user_name = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    normalized_user_name = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currency_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    prev_value = table.Column<decimal>(type: "numeric", nullable: false),
                    new_value = table.Column<decimal>(type: "numeric", nullable: false),
                    datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("currency_history_pk", x => x.id);
                    table.ForeignKey(
                        name: "currency_history_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "currency_to_usd",
                columns: table => new
                {
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    to_usd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currency_to_usd_pk", x => x.currency_id);
                    table.ForeignKey(
                        name: "currency_to_usd_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
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
                name: "articles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    article_number = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    normalized_article_number = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    article_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    packing_unit = table.Column<int>(type: "integer", nullable: true),
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    is_oe = table.Column<bool>(type: "boolean", nullable: false),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    indicator = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    popularity = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    articlename_tsv = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true, computedColumnSql: "to_tsvector('russian'::regconfig, (article_name)::text)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("articles_id_pk", x => x.id);
                    table.ForeignKey(
                        name: "articles_categories_id_fk",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "producer_id_fk",
                        column: x => x.producer_id,
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "producer_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    address_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    name_2 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    country = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    city = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    country_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    street = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    street_2 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    postal_country_code = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("producer_details_pk", x => x.id);
                    table.ForeignKey(
                        name: "producer_details_id_fk",
                        column: x => x.producer_id,
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "producers_other_names",
                columns: table => new
                {
                    producer_id = table.Column<int>(type: "integer", nullable: false),
                    producer_other_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    where_used = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("producers_other_names_pk", x => new { x.producer_id, x.producer_other_name, x.where_used });
                    table.ForeignKey(
                        name: "producers_other_names_producer_id_fk",
                        column: x => x.producer_id,
                        principalTable: "producer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "auth",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_permissions_pk", x => new { x.role_id, x.permission_name });
                    table.ForeignKey(
                        name: "role_permissions_permissions_name_fk",
                        column: x => x.permission_name,
                        principalSchema: "auth",
                        principalTable: "permissions",
                        principalColumn: "name");
                    table.ForeignKey(
                        name: "role_permissions_roles_id_fk",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    buyer_approved = table.Column<bool>(type: "boolean", nullable: false),
                    seller_approved = table.Column<bool>(type: "boolean", nullable: false),
                    signed_total_price = table.Column<string>(type: "text", nullable: false),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    is_canceled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("orders_pk", x => x.id);
                    table.ForeignKey(
                        name: "orders_currency_id_fk",
                        column: x => x.currency_id,
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
                    table.ForeignKey(
                        name: "orders_users_id_fk_2",
                        column: x => x.who_updated,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "storage_owners",
                columns: table => new
                {
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_owners_pk", x => new { x.storage_name, x.owner_id });
                    table.ForeignKey(
                        name: "storage_owners_storages_name_fk",
                        column: x => x.storage_name,
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_owners_users_id_fk",
                        column: x => x.owner_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_routes",
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
                    minimum_price = table.Column<decimal>(type: "numeric", nullable: true),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_routes_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_routes_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_routes_storages_name_fk",
                        column: x => x.from_storage_name,
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "storage_routes_storages_name_fk_2",
                        column: x => x.to_storage_name,
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
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_sum = table.Column<decimal>(type: "numeric", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    status = table.Column<string>(type: "character varying(28)", maxLength: 28, nullable: false),
                    who_made_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    receiver_balance_after_transaction = table.Column<decimal>(type: "numeric", nullable: false),
                    sender_balance_after_transaction = table.Column<decimal>(type: "numeric", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("transactions_pk", x => x.id);
                    table.ForeignKey(
                        name: "transactions_currency_id_fk",
                        column: x => x.currency_id,
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
                        name: "transactions_users_id_fk_3",
                        column: x => x.who_made_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transactions_users_id_fk_4",
                        column: x => x.deleted_by,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_balances",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('table_name_id_seq'::regclass)"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_balances_pk", x => x.id);
                    table.ForeignKey(
                        name: "user_balances_currency_id_fk",
                        column: x => x.currency_id,
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
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount = table.Column<decimal>(type: "numeric", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    email_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_emails_pk", x => x.id);
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
                    is_supplier = table.Column<bool>(type: "boolean", nullable: false),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    normalized_phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    phone_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_phones_pk", x => x.id);
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
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_roles_pk", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "user_roles_roles_id_fk",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id",
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
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_place = table.Column<string>(type: "text", nullable: false),
                    query = table.Column<string>(type: "jsonb", nullable: false),
                    search_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
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
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoke_reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
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
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    plate_number = table.Column<string>(type: "text", nullable: false),
                    manufacture = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    model = table.Column<string>(type: "character varying(125)", maxLength: 125, nullable: false),
                    modification = table.Column<string>(type: "text", nullable: true),
                    engine_code = table.Column<string>(type: "text", nullable: true),
                    production_year = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
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
                name: "article_characteristics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_characteristics_pk", x => x.id);
                    table.ForeignKey(
                        name: "article_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_coefficients",
                columns: table => new
                {
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    coefficient_name = table.Column<string>(type: "character varying(56)", maxLength: 56, nullable: false),
                    valid_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_coefficients_pk", x => new { x.article_id, x.coefficient_name });
                    table.ForeignKey(
                        name: "article_coefficients_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "article_coefficients_coefficients_name_fk",
                        column: x => x.coefficient_name,
                        principalTable: "coefficients",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "article_crosses",
                columns: table => new
                {
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    article_cross_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_crosses_pk", x => new { x.article_id, x.article_cross_id });
                    table.ForeignKey(
                        name: "article_crosses_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "article_crosses_articles_id_fk_2",
                        column: x => x.article_cross_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_ean",
                columns: table => new
                {
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    ean = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_ean_pk", x => new { x.article_id, x.ean });
                    table.ForeignKey(
                        name: "article_id___fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_images",
                columns: table => new
                {
                    path = table.Column<string>(type: "text", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_images_pk", x => x.path);
                    table.ForeignKey(
                        name: "article_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_sizes",
                columns: table => new
                {
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    length = table.Column<decimal>(type: "numeric", nullable: false),
                    width = table.Column<decimal>(type: "numeric", nullable: false),
                    height = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    volume_m3 = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_sizes_pk", x => x.article_id);
                    table.ForeignKey(
                        name: "article_sizes_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_supplier_buy_info",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    who_proposed = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    buy_price = table.Column<decimal>(type: "numeric", nullable: false),
                    delivery_id_days = table.Column<int>(type: "integer", nullable: false),
                    creation_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    current_supplier_stock = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_supplier_buy_info_pk", x => x.id);
                    table.ForeignKey(
                        name: "article_supplier_buy_info_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "article_supplier_buy_info_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "article_supplier_buy_info_users_id_fk",
                        column: x => x.who_proposed,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_weight",
                columns: table => new
                {
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("article_weight_pk", x => x.article_id);
                    table.ForeignKey(
                        name: "article_weight_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "articles_content",
                columns: table => new
                {
                    main_article_id = table.Column<int>(type: "integer", nullable: false),
                    inside_article_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("articles_content_pk", x => new { x.main_article_id, x.inside_article_id });
                    table.ForeignKey(
                        name: "articles_content_in_id___fk",
                        column: x => x.inside_article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "articles_content_out_id___fk",
                        column: x => x.main_article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "articles_pair",
                columns: table => new
                {
                    article_left = table.Column<int>(type: "integer", nullable: false),
                    article_right = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("articles_pair_pk", x => new { x.article_left, x.article_right });
                    table.ForeignKey(
                        name: "articles_pair_articles_id_fk",
                        column: x => x.article_left,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "articles_pair_articles_id_fk_2",
                        column: x => x.article_right,
                        principalTable: "articles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cart",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("cart_pk", x => new { x.user_id, x.article_id });
                    table.ForeignKey(
                        name: "cart_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
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
                name: "storage_content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    buy_price = table.Column<decimal>(type: "numeric", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    buy_price_in_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    created_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    purchase_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_content_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_content_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_storages_name_fk",
                        column: x => x.storage_name,
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "storage_content_reservations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    initial_count = table.Column<int>(type: "integer", nullable: false),
                    current_count = table.Column<int>(type: "integer", nullable: false),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    given_price = table.Column<decimal>(type: "numeric", nullable: true),
                    given_currency_id = table.Column<int>(type: "integer", nullable: true),
                    is_done = table.Column<bool>(type: "boolean", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    who_created = table.Column<Guid>(type: "uuid", nullable: false),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_content_reservations_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_content_reservations_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_reservations_currency_id_fk",
                        column: x => x.given_currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_movement",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    action_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    who_moved = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_movement_pk", x => x.id);
                    table.ForeignKey(
                        name: "storage_movement_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_movement_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_movement_storages_name_fk",
                        column: x => x.storage_name,
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_movement_users_id_fk",
                        column: x => x.who_moved,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
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
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_items_orders_id_fk",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    who_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    buyer_approved = table.Column<bool>(type: "boolean", nullable: false),
                    seller_approved = table.Column<bool>(type: "boolean", nullable: false),
                    signed_total_price = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_versions_pk", x => x.id);
                    table.ForeignKey(
                        name: "order_versions_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_versions_orders_id_fk",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_versions_users_id_fk",
                        column: x => x.who_updated,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    purchase_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creation_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    update_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    storage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    state = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_pk", x => x.id);
                    table.ForeignKey(
                        name: "purchase_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_storages_name_fk",
                        column: x => x.storage,
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_transactions_id_fk",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_users_id_fk",
                        column: x => x.created_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_users_id_fk_2",
                        column: x => x.supplier_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_users_id_fk_3",
                        column: x => x.updated_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    sale_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creation_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    update_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    main_storage_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    state = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sale_pk", x => x.id);
                    table.ForeignKey(
                        name: "sale_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_storages_name_fk",
                        column: x => x.main_storage_name,
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_transactions_id_fk",
                        column: x => x.transaction_id,
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
                    table.ForeignKey(
                        name: "sale_users_id_fk_2",
                        column: x => x.created_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_users_id_fk_3",
                        column: x => x.updated_user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction_versions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    sender_id = table.Column<Guid>(name: "sender_id ", type: "uuid", nullable: false),
                    receiver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_sum = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<string>(type: "character varying(28)", maxLength: 28, nullable: false),
                    transaction_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    version_created_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("transaction_versions_pk", x => x.id);
                    table.ForeignKey(
                        name: "transaction_versions_currency_id_fk",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transaction_versions_transactions_id_fk",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transaction_versions_users_id_fk",
                        column: x => x.receiver_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transaction_versions_users_id_fk_2",
                        column: x => x.sender_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_id = table.Column<string>(type: "text", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    total_sum = table.Column<decimal>(type: "numeric", nullable: false),
                    comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    storage_content_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchase_content_pk", x => x.id);
                    table.ForeignKey(
                        name: "purchase_content_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_content_purchase_id_fk",
                        column: x => x.purchase_id,
                        principalTable: "purchase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "purchase_content_storage_content_id_fk",
                        column: x => x.storage_content_id,
                        principalTable: "storage_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "purchase_logistics",
                columns: table => new
                {
                    purchase_id = table.Column<string>(type: "text", nullable: false),
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
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_logistics_purchase_id_fk",
                        column: x => x.purchase_id,
                        principalTable: "purchase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "purchase_logistics_storage_routes_id_fk",
                        column: x => x.route_id,
                        principalTable: "storage_routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "purchase_logistics_transactions_id_fk",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_id = table.Column<string>(type: "text", nullable: false),
                    article_id = table.Column<int>(type: "integer", nullable: false),
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
                        name: "sale_content_articles_id_fk",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_content_sale_id_fk",
                        column: x => x.sale_id,
                        principalTable: "sale",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_content_logistics",
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
                        principalTable: "purchase_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_content_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_content_id = table.Column<int>(type: "integer", nullable: false),
                    storage_content_id = table.Column<int>(type: "integer", nullable: true),
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
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "sale_content_details_sale_content_id_fk",
                        column: x => x.sale_content_id,
                        principalTable: "sale_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "sale_content_details_storage_content_id_fk",
                        column: x => x.storage_content_id,
                        principalTable: "storage_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "sale_content_details_storages_name_fk",
                        column: x => x.storage,
                        principalTable: "storages",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "article_characteristics_value_index",
                table: "article_characteristics",
                column: "value");

            migrationBuilder.CreateIndex(
                name: "article_id__index",
                table: "article_characteristics",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "article_coefficients_valid_till_index",
                table: "article_coefficients",
                column: "valid_till");

            migrationBuilder.CreateIndex(
                name: "IX_article_coefficients_coefficient_name",
                table: "article_coefficients",
                column: "coefficient_name");

            migrationBuilder.CreateIndex(
                name: "article_crosses_article_cross_id_index",
                table: "article_crosses",
                column: "article_cross_id");

            migrationBuilder.CreateIndex(
                name: "article_crosses_article_id_index",
                table: "article_crosses",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "article_ean_ean_index",
                table: "article_ean",
                column: "ean");

            migrationBuilder.CreateIndex(
                name: "article_ean_id__index",
                table: "article_ean",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "article_images_id__index",
                table: "article_images",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "article_supplier_buy_info_article_id_index",
                table: "article_supplier_buy_info",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "article_supplier_buy_info_creation_datetime_index",
                table: "article_supplier_buy_info",
                column: "creation_datetime");

            migrationBuilder.CreateIndex(
                name: "article_supplier_buy_info_who_proposed_index",
                table: "article_supplier_buy_info",
                column: "who_proposed");

            migrationBuilder.CreateIndex(
                name: "IX_article_supplier_buy_info_currency_id",
                table: "article_supplier_buy_info",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "articles_article_name_index",
                table: "articles",
                column: "article_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "articles_article_number_index",
                table: "articles",
                column: "article_number")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "articles_category_id_index",
                table: "articles",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "articles_normalized_article_number_producer_id_index",
                table: "articles",
                columns: new[] { "normalized_article_number", "producer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "articles_popularity_index",
                table: "articles",
                column: "popularity");

            migrationBuilder.CreateIndex(
                name: "articles_producer_id_index",
                table: "articles",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "articles_total_count_index",
                table: "articles",
                column: "total_count");

            migrationBuilder.CreateIndex(
                name: "IX_articles_articlename_tsv",
                table: "articles",
                column: "articlename_tsv")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "normalized_article_number__index",
                table: "articles",
                column: "normalized_article_number")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "article_main_inside_index",
                table: "articles_content",
                column: "inside_article_id");

            migrationBuilder.CreateIndex(
                name: "articles_content_main_article_id_index",
                table: "articles_content",
                column: "main_article_id");

            migrationBuilder.CreateIndex(
                name: "articles_pair_article_left_uindex",
                table: "articles_pair",
                column: "article_left",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_articles_pair_article_right",
                table: "articles_pair",
                column: "article_right");

            migrationBuilder.CreateIndex(
                name: "IX_cart_article_id",
                table: "cart",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "categories_name_index",
                table: "categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "currency_code_uindex",
                table: "currency",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_currency_sign_uindex",
                table: "currency",
                column: "currency_sign",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_name_uindex",
                table: "currency",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_short_name_uindex",
                table: "currency",
                column: "short_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_history_datetime_index",
                table: "currency_history",
                column: "datetime");

            migrationBuilder.CreateIndex(
                name: "currency_history_new_value_index",
                table: "currency_history",
                column: "new_value");

            migrationBuilder.CreateIndex(
                name: "currency_history_prev_value_index",
                table: "currency_history",
                column: "prev_value");

            migrationBuilder.CreateIndex(
                name: "IX_currency_history_currency_id",
                table: "currency_history",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                schema: "msg",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "order_items_article_id_index",
                table: "order_items",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "order_items_order_id_index",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_versions_currency_id",
                table: "order_versions",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_versions_who_updated",
                table: "order_versions",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "order_versions_order_id_id_index",
                table: "order_versions",
                columns: new[] { "order_id", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_who_updated",
                table: "orders",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "orders_buyer_approved_index",
                table: "orders",
                column: "buyer_approved");

            migrationBuilder.CreateIndex(
                name: "orders_currency_id_index",
                table: "orders",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "orders_is_canceled_index",
                table: "orders",
                column: "is_canceled");

            migrationBuilder.CreateIndex(
                name: "orders_seller_approved_index",
                table: "orders",
                column: "seller_approved");

            migrationBuilder.CreateIndex(
                name: "orders_status_index",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "orders_user_id_is_canceled_index",
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
                name: "producer_name_uindex",
                table: "producer",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "producer_details_producer_id_index",
                table: "producer_details",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_id_index",
                table: "producers_other_names",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "producers_other_names_producer_other_name_index",
                table: "producers_other_names",
                column: "producer_other_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "producers_other_names_where_used_index",
                table: "producers_other_names",
                column: "where_used")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "purchase_comment_index",
                table: "purchase",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "purchase_created_user_id_index",
                table: "purchase",
                column: "created_user_id");

            migrationBuilder.CreateIndex(
                name: "purchase_currency_id_index",
                table: "purchase",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "purchase_purchase_datetime_index",
                table: "purchase",
                column: "purchase_datetime");

            migrationBuilder.CreateIndex(
                name: "purchase_state_index",
                table: "purchase",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "purchase_storage_index",
                table: "purchase",
                column: "storage");

            migrationBuilder.CreateIndex(
                name: "purchase_supplier_id_index",
                table: "purchase",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "purchase_transaction_id_index",
                table: "purchase",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "purchase_updated_user_id_index",
                table: "purchase",
                column: "updated_user_id");

            migrationBuilder.CreateIndex(
                name: "purchase_content_article_id_index",
                table: "purchase_content",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "purchase_content_comment_index",
                table: "purchase_content",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "purchase_content_purchase_id_index",
                table: "purchase_content",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "purchase_content_storage_content_id_uindex",
                table: "purchase_content",
                column: "storage_content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "purchase_logistics_currency_id_index",
                table: "purchase_logistics",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "purchase_logistics_route_id_index",
                table: "purchase_logistics",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "purchase_logistics_transaction_id_uindex",
                table: "purchase_logistics",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_name",
                schema: "auth",
                table: "role_permissions",
                column: "permission_name");

            migrationBuilder.CreateIndex(
                name: "roles_normalized_name_uindex",
                schema: "auth",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "sale_buyer_id_index",
                table: "sale",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "sale_comment_index",
                table: "sale",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "sale_created_user_id_index",
                table: "sale",
                column: "created_user_id");

            migrationBuilder.CreateIndex(
                name: "sale_currency_id_index",
                table: "sale",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "sale_main_storage_name_index",
                table: "sale",
                column: "main_storage_name");

            migrationBuilder.CreateIndex(
                name: "sale_sale_datetime_index",
                table: "sale",
                column: "sale_datetime");

            migrationBuilder.CreateIndex(
                name: "sale_state_index",
                table: "sale",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "sale_transaction_id_index",
                table: "sale",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "sale_updated_user_id_index",
                table: "sale",
                column: "updated_user_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_article_id_index",
                table: "sale_content",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_comment_index",
                table: "sale_content",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "sale_content_sale_id_index",
                table: "sale_content",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_currency_id_index",
                table: "sale_content_details",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_sale_content_id_index",
                table: "sale_content_details",
                column: "sale_content_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_storage_content_id_index",
                table: "sale_content_details",
                column: "storage_content_id");

            migrationBuilder.CreateIndex(
                name: "sale_content_details_storage_index",
                table: "sale_content_details",
                column: "storage");

            migrationBuilder.CreateIndex(
                name: "storage_content_article_id_count_index",
                table: "storage_content",
                columns: new[] { "article_id", "count" });

            migrationBuilder.CreateIndex(
                name: "storage_content_article_id_index",
                table: "storage_content",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "storage_content_article_id_storage_name_index",
                table: "storage_content",
                columns: new[] { "article_id", "storage_name" });

            migrationBuilder.CreateIndex(
                name: "storage_content_buy_price_in_usd_index",
                table: "storage_content",
                column: "buy_price_in_usd");

            migrationBuilder.CreateIndex(
                name: "storage_content_buy_price_index",
                table: "storage_content",
                column: "buy_price");

            migrationBuilder.CreateIndex(
                name: "storage_content_currency_id_index",
                table: "storage_content",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "storage_content_purchase_datetime_index",
                table: "storage_content",
                column: "purchase_datetime");

            migrationBuilder.CreateIndex(
                name: "storage_content_storage_name_article_id_index",
                table: "storage_content",
                columns: new[] { "storage_name", "article_id" });

            migrationBuilder.CreateIndex(
                name: "storage_content_storage_name_index",
                table: "storage_content",
                column: "storage_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_storage_content_reservations_given_currency_id",
                table: "storage_content_reservations",
                column: "given_currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_storage_content_reservations_who_created",
                table: "storage_content_reservations",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "IX_storage_content_reservations_who_updated",
                table: "storage_content_reservations",
                column: "who_updated");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_article_id_index",
                table: "storage_content_reservations",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_article_id_is_done_index",
                table: "storage_content_reservations",
                columns: new[] { "article_id", "is_done" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_comment_index",
                table: "storage_content_reservations",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_create_at_index",
                table: "storage_content_reservations",
                column: "create_at");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_is_done_index",
                table: "storage_content_reservations",
                column: "is_done");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_updated_at_index",
                table: "storage_content_reservations",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_user_id_index",
                table: "storage_content_reservations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_user_id_is_done_index",
                table: "storage_content_reservations",
                columns: new[] { "user_id", "is_done" });

            migrationBuilder.CreateIndex(
                name: "storage_movement_action_type_index",
                table: "storage_movement",
                column: "action_type");

            migrationBuilder.CreateIndex(
                name: "storage_movement_article_id_index",
                table: "storage_movement",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "storage_movement_count_index",
                table: "storage_movement",
                column: "count");

            migrationBuilder.CreateIndex(
                name: "storage_movement_created_at_index",
                table: "storage_movement",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "storage_movement_currency_id_index",
                table: "storage_movement",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "storage_movement_price_index",
                table: "storage_movement",
                column: "price");

            migrationBuilder.CreateIndex(
                name: "storage_movement_storage_name_index",
                table: "storage_movement",
                column: "storage_name");

            migrationBuilder.CreateIndex(
                name: "storage_movement_who_moved_index",
                table: "storage_movement",
                column: "who_moved");

            migrationBuilder.CreateIndex(
                name: "storage_owners_owner_id_index",
                table: "storage_owners",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_storage_routes_to_storage_name",
                table: "storage_routes",
                column: "to_storage_name");

            migrationBuilder.CreateIndex(
                name: "storage_from_to_active_uindex",
                table: "storage_routes",
                columns: new[] { "from_storage_name", "to_storage_name", "is_active" },
                unique: true,
                filter: "(is_active = true)");

            migrationBuilder.CreateIndex(
                name: "storage_routes_carrier_id_index",
                table: "storage_routes",
                column: "carrier_id");

            migrationBuilder.CreateIndex(
                name: "storage_routes_currency_id_index",
                table: "storage_routes",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "storages_description_index",
                table: "storages",
                column: "description")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "storages_location_index",
                table: "storages",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "storages_type_index",
                table: "storages",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "transaction_versions_currency_id_index",
                table: "transaction_versions",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "transaction_versions_receiver_id_index",
                table: "transaction_versions",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "transaction_versions_sender_id _index",
                table: "transaction_versions",
                column: "sender_id ");

            migrationBuilder.CreateIndex(
                name: "transaction_versions_status_index",
                table: "transaction_versions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "transaction_versions_transaction_datetime_index",
                table: "transaction_versions",
                column: "transaction_datetime");

            migrationBuilder.CreateIndex(
                name: "transaction_versions_transaction_id_index",
                table: "transaction_versions",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "transaction_versions_transaction_id_version_uindex",
                table: "transaction_versions",
                columns: new[] { "transaction_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "transaction_versions_version_created_datetime_index",
                table: "transaction_versions",
                column: "version_created_datetime");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_currency_id",
                table: "transactions",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "transactions_creation_date_index",
                table: "transactions",
                column: "creation_date");

            migrationBuilder.CreateIndex(
                name: "transactions_deleted_by_index",
                table: "transactions",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "transactions_is_deleted_index",
                table: "transactions",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "transactions_receiver_id_index",
                table: "transactions",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "transactions_sender_id_index",
                table: "transactions",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "transactions_sender_id_receiver_id_index",
                table: "transactions",
                columns: new[] { "sender_id", "receiver_id" });

            migrationBuilder.CreateIndex(
                name: "transactions_status_index",
                table: "transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "transactions_transaction_datetime_id_index",
                table: "transactions",
                columns: new[] { "transaction_datetime", "id" });

            migrationBuilder.CreateIndex(
                name: "transactions_transaction_datetime_index",
                table: "transactions",
                column: "transaction_datetime");

            migrationBuilder.CreateIndex(
                name: "transactions_transaction_datetime_sender_id_receiver_id_idx",
                table: "transactions",
                column: "transaction_datetime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "transactions_who_made_user_id_index",
                table: "transactions",
                column: "who_made_user_id");

            migrationBuilder.CreateIndex(
                name: "user_balances_balance_index",
                table: "user_balances",
                column: "balance");

            migrationBuilder.CreateIndex(
                name: "user_balances_currency_id_index",
                table: "user_balances",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "user_balances_currency_id_user_id_uindex",
                table: "user_balances",
                columns: new[] { "currency_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_balances_user_id_index",
                table: "user_balances",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_emails_normalized_email_index",
                schema: "auth",
                table: "user_emails",
                column: "normalized_email")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_emails_normalized_email_uindex",
                schema: "auth",
                table: "user_emails",
                column: "normalized_email",
                unique: true);

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
                name: "user_info_is_supplier_index",
                schema: "auth",
                table: "user_info",
                column: "is_supplier");

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
                name: "user_phones_normalized_phone_index",
                schema: "auth",
                table: "user_phones",
                column: "normalized_phone")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_phones_normalized_phone_uindex",
                schema: "auth",
                table: "user_phones",
                column: "normalized_phone",
                unique: true);

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
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "user_search_history_search_date_time_index",
                table: "user_search_history",
                column: "search_date_time");

            migrationBuilder.CreateIndex(
                name: "user_search_history_search_place_index",
                table: "user_search_history",
                column: "search_place");

            migrationBuilder.CreateIndex(
                name: "user_search_history_user_id_index",
                table: "user_search_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_search_history_user_id_search_place_index",
                table: "user_search_history",
                columns: new[] { "user_id", "search_place" });

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
                name: "user_vehicles_comment_index",
                table: "user_vehicles",
                column: "comment")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_manufacture_index",
                table: "user_vehicles",
                column: "manufacture")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_model_index",
                table: "user_vehicles",
                column: "model")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "user_vehicles_plate_number_uindex",
                table: "user_vehicles",
                column: "plate_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_vehicles_user_id_index",
                table: "user_vehicles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_vehicles_vin_uindex",
                table: "user_vehicles",
                column: "vin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_normalized_user_name_index",
                schema: "auth",
                table: "users",
                column: "normalized_user_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "users_normalized_user_name_uindex",
                schema: "auth",
                table: "users",
                column: "normalized_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_characteristics");

            migrationBuilder.DropTable(
                name: "article_coefficients");

            migrationBuilder.DropTable(
                name: "article_crosses");

            migrationBuilder.DropTable(
                name: "article_ean");

            migrationBuilder.DropTable(
                name: "article_images");

            migrationBuilder.DropTable(
                name: "article_sizes");

            migrationBuilder.DropTable(
                name: "article_supplier_buy_info");

            migrationBuilder.DropTable(
                name: "article_weight");

            migrationBuilder.DropTable(
                name: "articles_content");

            migrationBuilder.DropTable(
                name: "articles_pair");

            migrationBuilder.DropTable(
                name: "cart");

            migrationBuilder.DropTable(
                name: "currency_history");

            migrationBuilder.DropTable(
                name: "currency_to_usd");

            migrationBuilder.DropTable(
                name: "default_settings");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "order_versions");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "producer_details");

            migrationBuilder.DropTable(
                name: "producers_other_names");

            migrationBuilder.DropTable(
                name: "purchase_content_logistics");

            migrationBuilder.DropTable(
                name: "purchase_logistics");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "sale_content_details");

            migrationBuilder.DropTable(
                name: "storage_content_reservations");

            migrationBuilder.DropTable(
                name: "storage_movement");

            migrationBuilder.DropTable(
                name: "storage_owners");

            migrationBuilder.DropTable(
                name: "transaction_versions");

            migrationBuilder.DropTable(
                name: "user_balances");

            migrationBuilder.DropTable(
                name: "user_discounts");

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
                name: "user_search_history");

            migrationBuilder.DropTable(
                name: "user_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_vehicles");

            migrationBuilder.DropTable(
                name: "coefficients");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "msg");

            migrationBuilder.DropTable(
                name: "purchase_content");

            migrationBuilder.DropTable(
                name: "storage_routes");

            migrationBuilder.DropTable(
                name: "sale_content");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "purchase");

            migrationBuilder.DropTable(
                name: "storage_content");

            migrationBuilder.DropTable(
                name: "sale");

            migrationBuilder.DropTable(
                name: "articles");

            migrationBuilder.DropTable(
                name: "storages");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "producer");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropSequence(
                name: "storage_movement_id_seq");

            migrationBuilder.DropSequence(
                name: "table_name_id_seq");
        }
    }
}

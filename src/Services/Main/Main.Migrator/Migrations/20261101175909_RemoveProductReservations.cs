using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve journal entries before removing the original entity and its discriminator.
            migrationBuilder.Sql("""
                UPDATE public.events
                SET json = json || jsonb_build_object(
                        'reservationId', reservation_id,
                        'legacyEventType', discriminator),
                    discriminator = 'ArchivedEvent'
                WHERE discriminator = 'ReservationManualChangeEvent';

                DELETE FROM auth.role_permissions
                WHERE permission IN ('ARTICLE.RESERVATIONS.CREATE', 'ARTICLE.RESERVATIONS.DELETE', 'ARTICLE.RESERVATIONS.EDIT',
                    'ARTICLE.RESERVATIONS.GET.ME', 'ARTICLE.RESERVATIONS.GET.ALL');
                DELETE FROM auth.user_permissions
                WHERE permission IN ('ARTICLE.RESERVATIONS.CREATE', 'ARTICLE.RESERVATIONS.DELETE', 'ARTICLE.RESERVATIONS.EDIT',
                    'ARTICLE.RESERVATIONS.GET.ME', 'ARTICLE.RESERVATIONS.GET.ALL');
                DELETE FROM auth.permissions
                WHERE name IN ('ARTICLE.RESERVATIONS.CREATE', 'ARTICLE.RESERVATIONS.DELETE', 'ARTICLE.RESERVATIONS.EDIT',
                    'ARTICLE.RESERVATIONS.GET.ME', 'ARTICLE.RESERVATIONS.GET.ALL');
                """);

            migrationBuilder.DropForeignKey(
                name: "reservation_manual_change_event_reservation_id_fk",
                schema: "public",
                table: "events");

            migrationBuilder.DropTable(
                name: "storage_content_reservations",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "reservation_manual_change_event_reservation_id_idx",
                schema: "public",
                table: "events");

            migrationBuilder.DropColumn(
                name: "reservation_id",
                schema: "public",
                table: "events");

            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                schema: "public",
                table: "events",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "discriminator",
                schema: "public",
                table: "events",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);

            migrationBuilder.AddColumn<int>(
                name: "reservation_id",
                schema: "public",
                table: "events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "storage_content_reservations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    current_count = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    proposed_currency_id = table.Column<int>(type: "integer", nullable: true),
                    proposed_price = table.Column<decimal>(type: "numeric", nullable: true),
                    reserved_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                        name: "storage_content_reservations_organization_id_fk",
                        column: x => x.organization_id,
                        principalSchema: "auth",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "storage_content_reservations_products_id_fk",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "reservation_manual_change_event_reservation_id_idx",
                schema: "public",
                table: "events",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_storage_content_reservations_proposed_currency_id",
                schema: "public",
                table: "storage_content_reservations",
                column: "proposed_currency_id");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.productreservation_who_created_idx",
                schema: "public",
                table: "storage_content_reservations",
                column: "who_created");

            migrationBuilder.CreateIndex(
                name: "main.entities.storage.productreservation_who_updated_idx",
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
                name: "storage_content_reservations_organization_id_status_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_product_id_status_index",
                schema: "public",
                table: "storage_content_reservations",
                columns: new[] { "product_id", "status" });

            migrationBuilder.CreateIndex(
                name: "storage_content_reservations_status_index",
                schema: "public",
                table: "storage_content_reservations",
                column: "status");

            migrationBuilder.AddForeignKey(
                name: "reservation_manual_change_event_reservation_id_fk",
                schema: "public",
                table: "events",
                column: "reservation_id",
                principalSchema: "public",
                principalTable: "storage_content_reservations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

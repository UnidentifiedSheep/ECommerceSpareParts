using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public.orders)
                        OR EXISTS (SELECT 1 FROM public.order_items) THEN
                        RAISE EXCEPTION 'Orders migration requires empty legacy orders and order_items; existing orders need an explicit data migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "order_items_articles_id_fk",
                schema: "public",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "orders_users_id_fk",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_buyer_approved_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_is_canceled_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_seller_approved_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_user_id_is_canceled_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "buyer_approved",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "is_canceled",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "seller_approved",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "signed_total_price",
                schema: "public",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "article_id",
                schema: "public",
                table: "order_items",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "locked_price",
                schema: "public",
                table: "order_items",
                newName: "unit_price");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                schema: "public",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmed_at",
                schema: "public",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "confirmed_by_user_id",
                schema: "public",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fulfillment_status",
                schema: "public",
                table: "orders",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                schema: "public",
                table: "orders",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "source",
                schema: "public",
                table: "orders",
                type: "text",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "signed_price",
                schema: "public",
                table: "order_items",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.DropPrimaryKey(
                name: "order_items_pk",
                schema: "public",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "public",
                table: "order_items");

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "public",
                table: "order_items",
                type: "integer",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "order_items_pk",
                schema: "public",
                table: "order_items",
                column: "id");

            migrationBuilder.AddColumn<string>(
                name: "price_origin",
                schema: "public",
                table: "order_items",
                type: "text",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "orders_confirmed_by_user_id_index",
                schema: "public",
                table: "orders",
                column: "confirmed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "orders_fulfillment_status_index",
                schema: "public",
                table: "orders",
                column: "fulfillment_status");

            migrationBuilder.CreateIndex(
                name: "orders_organization_id_index",
                schema: "public",
                table: "orders",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "orders_user_id_index",
                schema: "public",
                table: "orders",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "order_items_products_id_fk",
                schema: "public",
                table: "order_items",
                column: "product_id",
                principalSchema: "public",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "orders_confirmed_by_user_id_fk",
                schema: "public",
                table: "orders",
                column: "confirmed_by_user_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "orders_organization_id_fk",
                schema: "public",
                table: "orders",
                column: "organization_id",
                principalSchema: "auth",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "orders_users_id_fk",
                schema: "public",
                table: "orders",
                column: "user_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public.orders)
                        OR EXISTS (SELECT 1 FROM public.order_items) THEN
                        RAISE EXCEPTION 'Orders migration cannot be reversed while orders or order_items contain data.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "order_items_products_id_fk",
                schema: "public",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "orders_confirmed_by_user_id_fk",
                schema: "public",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "orders_organization_id_fk",
                schema: "public",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "orders_users_id_fk",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_confirmed_by_user_id_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_fulfillment_status_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_organization_id_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "orders_user_id_index",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "confirmed_by_user_id",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "fulfillment_status",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "organization_id",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "price_origin",
                schema: "public",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "source",
                schema: "public",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "product_id",
                schema: "public",
                table: "order_items",
                newName: "article_id");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                schema: "public",
                table: "order_items",
                newName: "locked_price");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                schema: "public",
                table: "orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "signed_total_price",
                schema: "public",
                table: "orders",
                type: "text",
                nullable: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "buyer_approved",
                schema: "public",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_canceled",
                schema: "public",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "seller_approved",
                schema: "public",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "signed_price",
                schema: "public",
                table: "order_items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.DropPrimaryKey(
                name: "order_items_pk",
                schema: "public",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "public",
                table: "order_items");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                schema: "public",
                table: "order_items",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "order_items_pk",
                schema: "public",
                table: "order_items",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "orders_buyer_approved_index",
                schema: "public",
                table: "orders",
                column: "buyer_approved");

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
                name: "orders_user_id_is_canceled_index",
                schema: "public",
                table: "orders",
                columns: new[] { "user_id", "is_canceled" });

            migrationBuilder.AddForeignKey(
                name: "order_items_articles_id_fk",
                schema: "public",
                table: "order_items",
                column: "article_id",
                principalSchema: "public",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "orders_users_id_fk",
                schema: "public",
                table: "orders",
                column: "user_id",
                principalSchema: "auth",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuditIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "main.entities.user.user_who_updated_idx",
                schema: "auth",
                table: "users",
                newName: "users_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.user.user_who_created_idx",
                schema: "auth",
                table: "users",
                newName: "users_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.usertoken_who_updated_idx",
                schema: "auth",
                table: "user_tokens",
                newName: "user_tokens_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.usertoken_who_created_idx",
                schema: "auth",
                table: "user_tokens",
                newName: "user_tokens_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.userrole_who_updated_idx",
                schema: "auth",
                table: "user_roles",
                newName: "user_roles_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.userrole_who_created_idx",
                schema: "auth",
                table: "user_roles",
                newName: "user_roles_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.user.userphone_who_updated_idx",
                schema: "auth",
                table: "user_phones",
                newName: "user_phones_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.user.userphone_who_created_idx",
                schema: "auth",
                table: "user_phones",
                newName: "user_phones_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.userpermission_who_updated_idx",
                schema: "auth",
                table: "user_permissions",
                newName: "user_permissions_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.userpermission_who_created_idx",
                schema: "auth",
                table: "user_permissions",
                newName: "user_permissions_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.user.useremail_who_updated_idx",
                schema: "auth",
                table: "user_emails",
                newName: "user_emails_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.user.useremail_who_created_idx",
                schema: "auth",
                table: "user_emails",
                newName: "user_emails_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.balance.transaction_who_updated_idx",
                schema: "public",
                table: "transactions",
                newName: "transactions_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.balance.transaction_who_created_idx",
                schema: "public",
                table: "transactions",
                newName: "transactions_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.enrichment.supplierproduct_who_updated_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                newName: "supplier_products_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.enrichment.supplierproduct_who_created_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                newName: "supplier_products_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storage_who_updated_idx",
                schema: "public",
                table: "storages",
                newName: "storages_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storage_who_created_idx",
                schema: "public",
                table: "storages",
                newName: "storages_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storagecontentlock_who_updated_idx",
                schema: "public",
                table: "storage_content_locks",
                newName: "storage_content_locks_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storagecontentlock_who_created_idx",
                schema: "public",
                table: "storage_content_locks",
                newName: "storage_content_locks_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storagecontent_who_updated_idx",
                schema: "public",
                table: "storage_content",
                newName: "storage_content_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.storage.storagecontent_who_created_idx",
                schema: "public",
                table: "storage_content",
                newName: "storage_content_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.settings.pricingsetting_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "settings_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.settings.pricingsetting_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "settings_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.sale.sale_who_updated_idx",
                schema: "public",
                table: "sale",
                newName: "sale_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.sale.sale_who_created_idx",
                schema: "public",
                table: "sale",
                newName: "sale_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.role_who_updated_idx",
                schema: "auth",
                table: "roles",
                newName: "roles_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.role_who_created_idx",
                schema: "auth",
                table: "roles",
                newName: "roles_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.purchase.purchase_who_updated_idx",
                schema: "public",
                table: "purchase",
                newName: "purchase_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.purchase.purchase_who_created_idx",
                schema: "public",
                table: "purchase",
                newName: "purchase_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.product_who_updated_idx",
                schema: "public",
                table: "products",
                newName: "products_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.product_who_created_idx",
                schema: "public",
                table: "products",
                newName: "products_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.offers.productpriceoption_who_updated_idx",
                schema: "public",
                table: "product_price_options",
                newName: "product_price_options_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.offers.productpriceoption_who_created_idx",
                schema: "public",
                table: "product_price_options",
                newName: "product_price_options_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.producer.producer_who_updated_idx",
                schema: "public",
                table: "producer",
                newName: "producer_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.producer.producer_who_created_idx",
                schema: "public",
                table: "producer",
                newName: "producer_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.offers.priceoffer_who_updated_idx",
                schema: "public",
                table: "price_offers",
                newName: "price_offers_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.offers.priceoffer_who_created_idx",
                schema: "public",
                table: "price_offers",
                newName: "price_offers_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.pricing.priceapplier_who_updated_idx",
                schema: "public",
                table: "price_appliers",
                newName: "price_appliers_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.pricing.priceapplier_who_created_idx",
                schema: "public",
                table: "price_appliers",
                newName: "price_appliers_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.pricing.priceapplierstate_who_updated_idx",
                schema: "public",
                table: "price_applier_states",
                newName: "price_applier_states_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.pricing.priceapplierstate_who_created_idx",
                schema: "public",
                table: "price_applier_states",
                newName: "price_applier_states_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.permission_who_updated_idx",
                schema: "auth",
                table: "permissions",
                newName: "permissions_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.auth.permission_who_created_idx",
                schema: "auth",
                table: "permissions",
                newName: "permissions_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.organization.organization_who_updated_idx",
                schema: "auth",
                table: "organizations",
                newName: "organizations_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.organization.organization_who_created_idx",
                schema: "auth",
                table: "organizations",
                newName: "organizations_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.organization.organizationmember_who_updated_idx",
                schema: "auth",
                table: "organization_members",
                newName: "organization_members_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.organization.organizationmember_who_created_idx",
                schema: "auth",
                table: "organization_members",
                newName: "organization_members_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.balance.organizationfinancialprofile_who_updated_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "organization_financial_profile_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.balance.organizationfinancialprofile_who_created_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "organization_financial_profile_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.balance.organizationbalance_who_updated_idx",
                schema: "public",
                table: "organization_balances",
                newName: "organization_balances_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.balance.organizationbalance_who_created_idx",
                schema: "public",
                table: "organization_balances",
                newName: "organization_balances_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.order.order_who_updated_idx",
                schema: "public",
                table: "orders",
                newName: "orders_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.order.order_who_created_idx",
                schema: "public",
                table: "orders",
                newName: "orders_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.markupgroup_who_updated_idx",
                schema: "public",
                table: "markup_group",
                newName: "markup_group_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "pricing.entities.markupgroup_who_created_idx",
                schema: "public",
                table: "markup_group",
                newName: "markup_group_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.singlerunjob_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "jobs_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.singlerunjob_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "jobs_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.jobschedule_who_updated_idx",
                schema: "job",
                table: "job_schedules",
                newName: "job_schedules_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "domain.commonentities.job.jobschedule_who_created_idx",
                schema: "job",
                table: "job_schedules",
                newName: "job_schedules_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.currency.currencyrate_who_updated_idx",
                schema: "public",
                table: "currency_rates",
                newName: "currency_rates_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.currency.currencyrate_who_created_idx",
                schema: "public",
                table: "currency_rates",
                newName: "currency_rates_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.enrichment.cataloguecandidate_who_updated_idx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                newName: "catalogue_candidates_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.product.enrichment.cataloguecandidate_who_created_idx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                newName: "catalogue_candidates_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.cart.cart_who_updated_idx",
                schema: "public",
                table: "cart",
                newName: "cart_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "main.entities.cart.cart_who_created_idx",
                schema: "public",
                table: "cart",
                newName: "cart_who_created_idx");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "users_who_updated_idx",
                schema: "auth",
                table: "users",
                newName: "main.entities.user.user_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "users_who_created_idx",
                schema: "auth",
                table: "users",
                newName: "main.entities.user.user_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "user_tokens_who_updated_idx",
                schema: "auth",
                table: "user_tokens",
                newName: "main.entities.auth.usertoken_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "user_tokens_who_created_idx",
                schema: "auth",
                table: "user_tokens",
                newName: "main.entities.auth.usertoken_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "user_roles_who_updated_idx",
                schema: "auth",
                table: "user_roles",
                newName: "main.entities.auth.userrole_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "user_roles_who_created_idx",
                schema: "auth",
                table: "user_roles",
                newName: "main.entities.auth.userrole_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "user_phones_who_updated_idx",
                schema: "auth",
                table: "user_phones",
                newName: "main.entities.user.userphone_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "user_phones_who_created_idx",
                schema: "auth",
                table: "user_phones",
                newName: "main.entities.user.userphone_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "user_permissions_who_updated_idx",
                schema: "auth",
                table: "user_permissions",
                newName: "main.entities.auth.userpermission_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "user_permissions_who_created_idx",
                schema: "auth",
                table: "user_permissions",
                newName: "main.entities.auth.userpermission_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "user_emails_who_updated_idx",
                schema: "auth",
                table: "user_emails",
                newName: "main.entities.user.useremail_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "user_emails_who_created_idx",
                schema: "auth",
                table: "user_emails",
                newName: "main.entities.user.useremail_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "transactions_who_updated_idx",
                schema: "public",
                table: "transactions",
                newName: "main.entities.balance.transaction_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "transactions_who_created_idx",
                schema: "public",
                table: "transactions",
                newName: "main.entities.balance.transaction_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "supplier_products_who_updated_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                newName: "main.entities.product.enrichment.supplierproduct_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "supplier_products_who_created_idx",
                schema: "catalogue_enrichment",
                table: "supplier_products",
                newName: "main.entities.product.enrichment.supplierproduct_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "storages_who_updated_idx",
                schema: "public",
                table: "storages",
                newName: "main.entities.storage.storage_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "storages_who_created_idx",
                schema: "public",
                table: "storages",
                newName: "main.entities.storage.storage_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "storage_content_locks_who_updated_idx",
                schema: "public",
                table: "storage_content_locks",
                newName: "main.entities.storage.storagecontentlock_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "storage_content_locks_who_created_idx",
                schema: "public",
                table: "storage_content_locks",
                newName: "main.entities.storage.storagecontentlock_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "storage_content_who_updated_idx",
                schema: "public",
                table: "storage_content",
                newName: "main.entities.storage.storagecontent_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "storage_content_who_created_idx",
                schema: "public",
                table: "storage_content",
                newName: "main.entities.storage.storagecontent_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "settings_who_updated_idx",
                schema: "public",
                table: "settings",
                newName: "pricing.entities.settings.pricingsetting_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "settings_who_created_idx",
                schema: "public",
                table: "settings",
                newName: "pricing.entities.settings.pricingsetting_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "sale_who_updated_idx",
                schema: "public",
                table: "sale",
                newName: "main.entities.sale.sale_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "sale_who_created_idx",
                schema: "public",
                table: "sale",
                newName: "main.entities.sale.sale_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "roles_who_updated_idx",
                schema: "auth",
                table: "roles",
                newName: "main.entities.auth.role_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "roles_who_created_idx",
                schema: "auth",
                table: "roles",
                newName: "main.entities.auth.role_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "purchase_who_updated_idx",
                schema: "public",
                table: "purchase",
                newName: "main.entities.purchase.purchase_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "purchase_who_created_idx",
                schema: "public",
                table: "purchase",
                newName: "main.entities.purchase.purchase_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "products_who_updated_idx",
                schema: "public",
                table: "products",
                newName: "main.entities.product.product_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "products_who_created_idx",
                schema: "public",
                table: "products",
                newName: "main.entities.product.product_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "product_price_options_who_updated_idx",
                schema: "public",
                table: "product_price_options",
                newName: "pricing.entities.offers.productpriceoption_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "product_price_options_who_created_idx",
                schema: "public",
                table: "product_price_options",
                newName: "pricing.entities.offers.productpriceoption_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "producer_who_updated_idx",
                schema: "public",
                table: "producer",
                newName: "main.entities.producer.producer_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "producer_who_created_idx",
                schema: "public",
                table: "producer",
                newName: "main.entities.producer.producer_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "price_offers_who_updated_idx",
                schema: "public",
                table: "price_offers",
                newName: "pricing.entities.offers.priceoffer_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "price_offers_who_created_idx",
                schema: "public",
                table: "price_offers",
                newName: "pricing.entities.offers.priceoffer_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "price_appliers_who_updated_idx",
                schema: "public",
                table: "price_appliers",
                newName: "pricing.entities.pricing.priceapplier_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "price_appliers_who_created_idx",
                schema: "public",
                table: "price_appliers",
                newName: "pricing.entities.pricing.priceapplier_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "price_applier_states_who_updated_idx",
                schema: "public",
                table: "price_applier_states",
                newName: "pricing.entities.pricing.priceapplierstate_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "price_applier_states_who_created_idx",
                schema: "public",
                table: "price_applier_states",
                newName: "pricing.entities.pricing.priceapplierstate_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "permissions_who_updated_idx",
                schema: "auth",
                table: "permissions",
                newName: "main.entities.auth.permission_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "permissions_who_created_idx",
                schema: "auth",
                table: "permissions",
                newName: "main.entities.auth.permission_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "organizations_who_updated_idx",
                schema: "auth",
                table: "organizations",
                newName: "main.entities.organization.organization_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "organizations_who_created_idx",
                schema: "auth",
                table: "organizations",
                newName: "main.entities.organization.organization_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "organization_members_who_updated_idx",
                schema: "auth",
                table: "organization_members",
                newName: "main.entities.organization.organizationmember_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "organization_members_who_created_idx",
                schema: "auth",
                table: "organization_members",
                newName: "main.entities.organization.organizationmember_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "organization_financial_profile_who_updated_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "main.entities.balance.organizationfinancialprofile_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "organization_financial_profile_who_created_idx",
                schema: "public",
                table: "organization_financial_profile",
                newName: "main.entities.balance.organizationfinancialprofile_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "organization_balances_who_updated_idx",
                schema: "public",
                table: "organization_balances",
                newName: "main.entities.balance.organizationbalance_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "organization_balances_who_created_idx",
                schema: "public",
                table: "organization_balances",
                newName: "main.entities.balance.organizationbalance_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "orders_who_updated_idx",
                schema: "public",
                table: "orders",
                newName: "main.entities.order.order_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "orders_who_created_idx",
                schema: "public",
                table: "orders",
                newName: "main.entities.order.order_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "markup_group_who_updated_idx",
                schema: "public",
                table: "markup_group",
                newName: "pricing.entities.markupgroup_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "markup_group_who_created_idx",
                schema: "public",
                table: "markup_group",
                newName: "pricing.entities.markupgroup_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "jobs_who_updated_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.singlerunjob_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "jobs_who_created_idx",
                schema: "job",
                table: "jobs",
                newName: "domain.commonentities.job.singlerunjob_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "job_schedules_who_updated_idx",
                schema: "job",
                table: "job_schedules",
                newName: "domain.commonentities.job.jobschedule_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "job_schedules_who_created_idx",
                schema: "job",
                table: "job_schedules",
                newName: "domain.commonentities.job.jobschedule_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "currency_rates_who_updated_idx",
                schema: "public",
                table: "currency_rates",
                newName: "main.entities.currency.currencyrate_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "currency_rates_who_created_idx",
                schema: "public",
                table: "currency_rates",
                newName: "main.entities.currency.currencyrate_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "catalogue_candidates_who_updated_idx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                newName: "main.entities.product.enrichment.cataloguecandidate_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "catalogue_candidates_who_created_idx",
                schema: "catalogue_enrichment",
                table: "catalogue_candidates",
                newName: "main.entities.product.enrichment.cataloguecandidate_who_created_idx");

            migrationBuilder.RenameIndex(
                name: "cart_who_updated_idx",
                schema: "public",
                table: "cart",
                newName: "main.entities.cart.cart_who_updated_idx");

            migrationBuilder.RenameIndex(
                name: "cart_who_created_idx",
                schema: "public",
                table: "cart",
                newName: "main.entities.cart.cart_who_created_idx");
        }
    }
}

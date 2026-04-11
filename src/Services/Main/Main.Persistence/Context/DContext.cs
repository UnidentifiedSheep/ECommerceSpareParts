using System.Reflection;
using Main.Entities;
using Main.Entities.Product;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using Persistence.Interceptors;

namespace Main.Persistence.Context;

public partial class DContext : DbContext
{
    public DContext()
    {
    }

    public DContext(DbContextOptions<DContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Articles { get; set; }

    public virtual DbSet<ProductCharacteristic> ArticleCharacteristics { get; set; }

    public virtual DbSet<ProductCoefficient> ArticleCoefficients { get; set; }

    public virtual DbSet<ProductEan> ArticleEans { get; set; }

    public virtual DbSet<ProductImage> ArticleImages { get; set; }

    public virtual DbSet<ArticleSize> ArticleSizes { get; set; }

    public virtual DbSet<ProductSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; }

    public virtual DbSet<ProductWeight> ArticleWeights { get; set; }

    public virtual DbSet<ProductContent> ArticlesContents { get; set; }

    public virtual DbSet<ProductPair> ArticlesPairs { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coefficient> Coefficients { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<CurrencyHistory> CurrencyHistories { get; set; }

    public virtual DbSet<CurrencyToUsd> CurrencyToUsds { get; set; }

    public virtual DbSet<DefaultSetting> DefaultSettings { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderVersion> OrderVersions { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<ProducersOtherName> ProducersOtherNames { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<PurchaseContent> PurchaseContents { get; set; }

    public virtual DbSet<PurchaseContentLogistic> PurchaseContentLogistics { get; set; }

    public virtual DbSet<PurchaseLogistic> PurchaseLogistics { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleContent> SaleContents { get; set; }

    public virtual DbSet<SaleContentDetail> SaleContentDetails { get; set; }

    public virtual DbSet<Storage> Storages { get; set; }

    public virtual DbSet<StorageContent> StorageContents { get; set; }

    public virtual DbSet<StorageContentReservation> StorageContentReservations { get; set; }

    public virtual DbSet<StorageMovement> StorageMovements { get; set; }

    public virtual DbSet<StorageOwner> StorageOwners { get; set; }

    public virtual DbSet<StorageRoute> StorageRoutes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionVersion> TransactionVersions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBalance> UserBalances { get; set; }

    public virtual DbSet<UserDiscount> UserDiscounts { get; set; }

    public virtual DbSet<UserEmail> UserEmails { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<UserPermission> UserPermissions { get; set; }

    public virtual DbSet<UserPhone> UserPhones { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSearchHistory> UserSearchHistories { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    public virtual DbSet<UserVehicle> UserVehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(new SelectForUpdateCommandInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();

        modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", "msg");
        modelBuilder.Entity<OutboxState>().ToTable("OutboxState", "msg");
        modelBuilder.Entity<InboxState>().ToTable("InboxState", "msg");

        modelBuilder
            .HasPostgresEnum("car_types", new[] { "PassengerCar", "CommercialVehicle", "Motorbike" })
            .HasPostgresExtension("dblink")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(GetType())!);

        modelBuilder.AddFieldsForAuditableEntities();
        

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_pk");

            entity.ToTable("sale");

            entity.HasIndex(e => e.BuyerId, "sale_buyer_id_index");

            entity.HasIndex(e => e.Comment, "sale_comment_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasIndex(e => e.CreatedUserId, "sale_created_user_id_index");

            entity.HasIndex(e => e.CurrencyId, "sale_currency_id_index");

            entity.HasIndex(e => e.MainStorageName, "sale_main_storage_name_index");

            entity.HasIndex(e => e.SaleDatetime, "sale_sale_datetime_index");

            entity.HasIndex(e => e.State, "sale_state_index");

            entity.HasIndex(e => e.TransactionId, "sale_transaction_id_index");

            entity.HasIndex(e => e.UpdatedUserId, "sale_updated_user_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(256)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");
            entity.Property(e => e.CreationDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.MainStorageName)
                .HasMaxLength(128)
                .HasColumnName("main_storage_name");
            entity.Property(e => e.SaleDatetime).HasColumnName("sale_datetime");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdateDatetime).HasColumnName("update_datetime");
            entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

            entity.HasOne(d => d.Buyer).WithMany(p => p.SaleBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_users_id_fk");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.SaleCreatedUsers)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_users_id_fk_2");

            entity.HasOne(d => d.Currency).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_currency_id_fk");

            entity.HasOne(d => d.MainStorageNameNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.MainStorageName)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_storages_name_fk");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Sales)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_transactions_id_fk");

            entity.HasOne(d => d.UpdatedUser).WithMany(p => p.SaleUpdatedUsers)
                .HasForeignKey(d => d.UpdatedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_users_id_fk_3");
        });

        modelBuilder.Entity<SaleContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_content_pk");

            entity.ToTable("sale_content");

            entity.HasIndex(e => e.ArticleId, "sale_content_article_id_index");

            entity.HasIndex(e => e.Comment, "sale_content_comment_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasIndex(e => e.SaleId, "sale_content_sale_id_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(256)
                .HasColumnName("comment");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.TotalSum).HasColumnName("total_sum");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleContents)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_content_articles_id_fk");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleContents)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("sale_content_sale_id_fk");
        });

        modelBuilder.Entity<SaleContentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_content_details_pk");

            entity.ToTable("sale_content_details");

            entity.HasIndex(e => e.CurrencyId, "sale_content_details_currency_id_index");

            entity.HasIndex(e => e.SaleContentId, "sale_content_details_sale_content_id_index");

            entity.HasIndex(e => e.StorageContentId, "sale_content_details_storage_content_id_index");

            entity.HasIndex(e => e.Storage, "sale_content_details_storage_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.PurchaseDatetime).HasColumnName("purchase_datetime");
            entity.Property(e => e.SaleContentId).HasColumnName("sale_content_id");
            entity.Property(e => e.Storage)
                .HasMaxLength(128)
                .HasColumnName("storage");
            entity.Property(e => e.StorageContentId).HasColumnName("storage_content_id");

            entity.HasOne(d => d.Currency).WithMany(p => p.SaleContentDetails)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_content_details_currency_id_fk");

            entity.HasOne(d => d.SaleContent).WithMany(p => p.SaleContentDetails)
                .HasForeignKey(d => d.SaleContentId)
                .HasConstraintName("sale_content_details_sale_content_id_fk");

            entity.HasOne(d => d.StorageNavigation).WithMany(p => p.SaleContentDetails)
                .HasForeignKey(d => d.Storage)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_content_details_storages_name_fk");

            entity.HasOne(d => d.StorageContent).WithMany(p => p.SaleContentDetails)
                .HasForeignKey(d => d.StorageContentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("sale_content_details_storage_content_id_fk");
        });

        modelBuilder.Entity<Storage>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("storages_pk");

            entity.ToTable("storages");

            entity.HasIndex(e => e.Description, "storages_description_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasIndex(e => e.Location, "storages_location_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasIndex(e => e.Type, "storages_type_index");

            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(256)
                .HasColumnName("location");
            entity.Property(e => e.Type)
                .HasMaxLength(24)
                .HasColumnName("type");
        });

        modelBuilder.Entity<StorageContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("storage_content_pk");

            entity.ToTable("storage_content");

            entity.HasIndex(e => new { e.ArticleId, e.Count }, "storage_content_article_id_count_index");

            entity.HasIndex(e => e.ArticleId, "storage_content_article_id_index");

            entity.HasIndex(e => new { e.ArticleId, e.StorageName }, "storage_content_article_id_storage_name_index");

            entity.HasIndex(e => e.BuyPriceInUsd, "storage_content_buy_price_in_usd_index");

            entity.HasIndex(e => e.BuyPrice, "storage_content_buy_price_index");

            entity.HasIndex(e => e.CurrencyId, "storage_content_currency_id_index");

            entity.HasIndex(e => e.PurchaseDatetime, "storage_content_purchase_datetime_index");

            entity.HasIndex(e => new { e.StorageName, e.ArticleId }, "storage_content_storage_name_article_id_index");

            entity.HasIndex(e => e.StorageName, "storage_content_storage_name_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.BuyPriceInUsd).HasColumnName("buy_price_in_usd");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.PurchaseDatetime).HasColumnName("purchase_datetime");
            entity.Property(e => e.StorageName)
                .HasMaxLength(128)
                .HasColumnName("storage_name");

            entity.HasOne(d => d.Product).WithMany(p => p.StorageContents)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_content_articles_id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.StorageContents)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_content_currency_id_fk");

            entity.HasOne(d => d.StorageNameNavigation).WithMany(p => p.StorageContents)
                .HasForeignKey(d => d.StorageName)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_content_storages_name_fk");
        });

        modelBuilder.Entity<StorageContentReservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("storage_content_reservations_pk");

            entity.ToTable("storage_content_reservations");

            entity.HasIndex(e => e.GivenCurrencyId, "IX_storage_content_reservations_given_currency_id");

            entity.HasIndex(e => e.WhoCreated, "IX_storage_content_reservations_who_created");

            entity.HasIndex(e => e.WhoUpdated, "IX_storage_content_reservations_who_updated");

            entity.HasIndex(e => e.ArticleId, "storage_content_reservations_article_id_index");

            entity.HasIndex(e => new { e.ArticleId, e.IsDone },
                "storage_content_reservations_article_id_is_done_index");

            entity.HasIndex(e => e.Comment, "storage_content_reservations_comment_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasIndex(e => e.CreateAt, "storage_content_reservations_create_at_index");

            entity.HasIndex(e => e.IsDone, "storage_content_reservations_is_done_index");

            entity.HasIndex(e => e.UpdatedAt, "storage_content_reservations_updated_at_index");

            entity.HasIndex(e => e.UserId, "storage_content_reservations_user_id_index");

            entity.HasIndex(e => new { e.UserId, e.IsDone }, "storage_content_reservations_user_id_is_done_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("create_at");
            entity.Property(e => e.CurrentCount).HasColumnName("current_count");
            entity.Property(e => e.GivenCurrencyId).HasColumnName("given_currency_id");
            entity.Property(e => e.GivenPrice).HasColumnName("given_price");
            entity.Property(e => e.InitialCount).HasColumnName("initial_count");
            entity.Property(e => e.IsDone).HasColumnName("is_done");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WhoCreated).HasColumnName("who_created");
            entity.Property(e => e.WhoUpdated).HasColumnName("who_updated");

            entity.HasOne(d => d.Product).WithMany(p => p.StorageContentReservations)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_content_reservations_articles_id_fk");

            entity.HasOne(d => d.GivenCurrency).WithMany(p => p.StorageContentReservations)
                .HasForeignKey(d => d.GivenCurrencyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("storage_content_reservations_currency_id_fk");

            entity.HasOne(d => d.User).WithMany(p => p.StorageContentReservationUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("storage_content_reservations_users_id_fk");

            entity.HasOne(d => d.WhoCreatedNavigation).WithMany(p => p.StorageContentReservationWhoCreatedNavigations)
                .HasForeignKey(d => d.WhoCreated)
                .HasConstraintName("storage_content_reservations_users_id_fk_3");

            entity.HasOne(d => d.WhoUpdatedNavigation).WithMany(p => p.StorageContentReservationWhoUpdatedNavigations)
                .HasForeignKey(d => d.WhoUpdated)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("storage_content_reservations_users_id_fk_2");
        });

        modelBuilder.Entity<StorageMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("storage_movement_pk");

            entity.ToTable("storage_movement");

            entity.HasIndex(e => e.ActionType, "storage_movement_action_type_index");

            entity.HasIndex(e => e.ArticleId, "storage_movement_article_id_index");

            entity.HasIndex(e => e.Count, "storage_movement_count_index");

            entity.HasIndex(e => e.CreatedAt, "storage_movement_created_at_index");

            entity.HasIndex(e => e.CurrencyId, "storage_movement_currency_id_index");

            entity.HasIndex(e => e.Price, "storage_movement_price_index");

            entity.HasIndex(e => e.StorageName, "storage_movement_storage_name_index");

            entity.HasIndex(e => e.WhoMoved, "storage_movement_who_moved_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(24)
                .HasColumnName("action_type");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.StorageName)
                .HasMaxLength(128)
                .HasColumnName("storage_name");
            entity.Property(e => e.WhoMoved).HasColumnName("who_moved");

            entity.HasOne(d => d.Product).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_movement_articles_id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_movement_currency_id_fk");

            entity.HasOne(d => d.StorageNameNavigation).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.StorageName)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_movement_storages_name_fk");

            entity.HasOne(d => d.WhoMovedNavigation).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.WhoMoved)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_movement_users_id_fk");
        });

        modelBuilder.Entity<StorageOwner>(entity =>
        {
            entity.HasKey(e => new { e.StorageName, e.OwnerId }).HasName("storage_owners_pk");

            entity.ToTable("storage_owners");

            entity.HasIndex(e => e.OwnerId, "storage_owners_owner_id_index");

            entity.Property(e => e.StorageName)
                .HasMaxLength(128)
                .HasColumnName("storage_name");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Owner).WithMany(p => p.StorageOwners)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("storage_owners_users_id_fk");

            entity.HasOne(d => d.StorageNameNavigation).WithMany(p => p.StorageOwners)
                .HasForeignKey(d => d.StorageName)
                .HasConstraintName("storage_owners_storages_name_fk");
        });

        modelBuilder.Entity<StorageRoute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("storage_routes_pk");

            entity.ToTable("storage_routes");

            entity.HasIndex(e => new { e.FromStorageName, e.ToStorageName, e.IsActive },
                    "storage_from_to_active_uindex")
                .IsUnique()
                .HasFilter("(is_active = true)");

            entity.HasIndex(e => e.CarrierId, "storage_routes_carrier_id_index");

            entity.HasIndex(e => e.CurrencyId, "storage_routes_currency_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CarrierId).HasColumnName("carrier_id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DeliveryTimeMinutes).HasColumnName("delivery_time_minutes");
            entity.Property(e => e.DistanceM).HasColumnName("distance_m");
            entity.Property(e => e.FromStorageName)
                .HasMaxLength(128)
                .HasColumnName("from_storage_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MinimumPrice).HasColumnName("minimum_price");
            entity.Property(e => e.PriceKg).HasColumnName("price_kg");
            entity.Property(e => e.PricePerM3).HasColumnName("price_per_m3");
            entity.Property(e => e.PricePerOrder).HasColumnName("price_per_order");
            entity.Property(e => e.PricingModel)
                .HasMaxLength(24)
                .HasColumnName("pricing_model");
            entity.Property(e => e.RouteType)
                .HasMaxLength(24)
                .HasColumnName("route_type");
            entity.Property(e => e.ToStorageName)
                .HasMaxLength(128)
                .HasColumnName("to_storage_name");

            entity.HasOne(d => d.Carrier).WithMany(p => p.StorageRoutes)
                .HasForeignKey(d => d.CarrierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("storage_routes_users_id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.StorageRoutes)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_routes_currency_id_fk");

            entity.HasOne(d => d.FromStorageNameNavigation).WithMany(p => p.StorageRouteFromStorageNameNavigations)
                .HasForeignKey(d => d.FromStorageName)
                .HasConstraintName("storage_routes_storages_name_fk");

            entity.HasOne(d => d.ToStorageNameNavigation).WithMany(p => p.StorageRouteToStorageNameNavigations)
                .HasForeignKey(d => d.ToStorageName)
                .HasConstraintName("storage_routes_storages_name_fk_2");
        });
        

        modelBuilder.HasSequence<int>("storage_movement_id_seq");

        modelBuilder.AllDateTimesToUtc()
            .AllEnumsToString();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
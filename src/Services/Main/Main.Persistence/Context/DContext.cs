using Main.Entities;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(new SelectForUpdateCommandInterceptor());
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<ArticleCharacteristic> ArticleCharacteristics { get; set; }

    public virtual DbSet<ArticleCoefficient> ArticleCoefficients { get; set; }

    public virtual DbSet<ArticleEan> ArticleEans { get; set; }

    public virtual DbSet<ArticleImage> ArticleImages { get; set; }

    public virtual DbSet<ArticleSize> ArticleSizes { get; set; }

    public virtual DbSet<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; }

    public virtual DbSet<ArticleWeight> ArticleWeights { get; set; }

    public virtual DbSet<ArticlesContent> ArticlesContents { get; set; }

    public virtual DbSet<ArticlesPair> ArticlesPairs { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coefficient> Coefficients { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<CurrencyHistory> CurrencyHistories { get; set; }

    public virtual DbSet<CurrencyToUsd> CurrencyToUsds { get; set; }

    public virtual DbSet<DefaultSetting> DefaultSettings { get; set; }

    public virtual DbSet<MarkupGroup> MarkupGroups { get; set; }

    public virtual DbSet<MarkupRange> MarkupRanges { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderVersion> OrderVersions { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<ProducerDetail> ProducerDetails { get; set; }

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

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("articles_id_pk");

            entity.ToTable("articles");
            
            entity.Property<NpgsqlTsVector>("articlename_tsv")
                .HasColumnType("tsvector")
                .HasComputedColumnSql("to_tsvector('russian'::regconfig, (article_name)::text)", true);

            entity.HasIndex("articlename_tsv")
                .HasMethod("gin");
            
            entity.HasIndex(e => e.ArticleName, "articles_article_name_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.ArticleNumber, "articles_article_number_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.CategoryId, "articles_category_id_index");

            entity.HasIndex(e => new { e.NormalizedArticleNumber, e.ProducerId }, "articles_normalized_article_number_producer_id_index").IsUnique();

            entity.HasIndex(e => e.ProducerId, "articles_producer_id_index");

            entity.HasIndex(e => e.TotalCount, "articles_total_count_index");

            entity.HasIndex(e => e.NormalizedArticleNumber, "normalized_article_number__index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleName)
                .HasMaxLength(255)
                .HasColumnName("article_name");
            entity.Property(e => e.ArticleNumber)
                .HasMaxLength(128)
                .HasColumnName("article_number");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Indicator)
                .HasMaxLength(24)
                .HasColumnName("indicator");
            entity.Property(e => e.IsOe).HasColumnName("is_oe");
            entity.Property(e => e.IsValid)
                .HasDefaultValue(true)
                .HasColumnName("is_valid");
            entity.Property(e => e.NormalizedArticleNumber)
                .HasMaxLength(128)
                .HasColumnName("normalized_article_number");
            entity.Property(e => e.PackingUnit).HasColumnName("packing_unit");
            entity.Property(e => e.ProducerId).HasColumnName("producer_id");
            entity.Property(e => e.TotalCount).HasColumnName("total_count");

            entity.HasOne(d => d.Category).WithMany(p => p.Articles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("articles_categories_id_fk");

            entity.HasOne(d => d.Producer).WithMany(p => p.Articles)
                .HasForeignKey(d => d.ProducerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("producer_id_fk");

            entity.HasMany(d => d.ArticleCrosses).WithMany(p => p.Articles)
                .UsingEntity<Dictionary<string, object>>(
                    "ArticleCross",
                    r => r.HasOne<Article>().WithMany()
                        .HasForeignKey("ArticleCrossId")
                        .HasConstraintName("article_crosses_articles_id_fk_2"),
                    l => l.HasOne<Article>().WithMany()
                        .HasForeignKey("ArticleId")
                        .HasConstraintName("article_crosses_articles_id_fk"),
                    j =>
                    {
                        j.HasKey("ArticleId", "ArticleCrossId").HasName("article_crosses_pk");
                        j.ToTable("article_crosses");
                        j.HasIndex(new[] { "ArticleCrossId" }, "article_crosses_article_cross_id_index");
                        j.HasIndex(new[] { "ArticleId" }, "article_crosses_article_id_index");
                        j.IndexerProperty<int>("ArticleId").HasColumnName("article_id");
                        j.IndexerProperty<int>("ArticleCrossId").HasColumnName("article_cross_id");
                    });

            entity.HasMany(d => d.Articles).WithMany(p => p.ArticleCrosses)
                .UsingEntity<Dictionary<string, object>>(
                    "ArticleCross",
                    r => r.HasOne<Article>().WithMany()
                        .HasForeignKey("ArticleId")
                        .HasConstraintName("article_crosses_articles_id_fk"),
                    l => l.HasOne<Article>().WithMany()
                        .HasForeignKey("ArticleCrossId")
                        .HasConstraintName("article_crosses_articles_id_fk_2"),
                    j =>
                    {
                        j.HasKey("ArticleId", "ArticleCrossId").HasName("article_crosses_pk");
                        j.ToTable("article_crosses");
                        j.HasIndex(new[] { "ArticleCrossId" }, "article_crosses_article_cross_id_index");
                        j.HasIndex(new[] { "ArticleId" }, "article_crosses_article_id_index");
                        j.IndexerProperty<int>("ArticleId").HasColumnName("article_id");
                        j.IndexerProperty<int>("ArticleCrossId").HasColumnName("article_cross_id");
                    });
        });

        modelBuilder.Entity<ArticleCharacteristic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("article_characteristics_pk");

            entity.ToTable("article_characteristics");

            entity.HasIndex(e => e.Value, "article_characteristics_value_index");

            entity.HasIndex(e => e.ArticleId, "article_id__index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.Value)
                .HasMaxLength(128)
                .HasColumnName("value");

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleCharacteristics)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("article_id_fk");
        });

        modelBuilder.Entity<ArticleCoefficient>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.CoefficientName }).HasName("article_coefficients_pk");

            entity.ToTable("article_coefficients");

            entity.HasIndex(e => e.ValidTill, "article_coefficients_valid_till_index");

            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.CoefficientName)
                .HasMaxLength(56)
                .HasColumnName("coefficient_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ValidTill).HasColumnName("valid_till");

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleCoefficients)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("article_coefficients_articles_id_fk");

            entity.HasOne(d => d.CoefficientNameNavigation).WithMany(p => p.ArticleCoefficients)
                .HasForeignKey(d => d.CoefficientName)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("article_coefficients_coefficients_name_fk");
        });

        modelBuilder.Entity<ArticleEan>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.Ean }).HasName("article_ean_pk");

            entity.ToTable("article_ean");

            entity.HasIndex(e => e.Ean, "article_ean_ean_index");

            entity.HasIndex(e => e.ArticleId, "article_ean_id__index");

            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Ean)
                .HasMaxLength(24)
                .HasColumnName("ean");

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleEans)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("article_id___fk");
        });

        modelBuilder.Entity<ArticleImage>(entity =>
        {
            entity.HasKey(e => e.Path).HasName("article_images_pk");

            entity.ToTable("article_images");

            entity.HasIndex(e => e.ArticleId, "article_images_id__index");

            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .HasColumnName("description");

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleImages)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("article_id_fk");
        });

        modelBuilder.Entity<ArticleSize>(entity =>
        {
            entity.HasKey(e => e.ArticleId).HasName("article_sizes_pk");

            entity.ToTable("article_sizes");

            entity.Property(e => e.ArticleId)
                .ValueGeneratedNever()
                .HasColumnName("article_id");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Length).HasColumnName("length");
            entity.Property(e => e.Unit)
                .HasMaxLength(24)
                .HasColumnName("unit");
            entity.Property(e => e.VolumeM3).HasColumnName("volume_m3");
            entity.Property(e => e.Width).HasColumnName("width");

            entity.HasOne(d => d.Article).WithOne(p => p.ArticleSize)
                .HasForeignKey<ArticleSize>(d => d.ArticleId)
                .HasConstraintName("article_sizes_articles_id_fk");
        });

        modelBuilder.Entity<ArticleSupplierBuyInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("article_supplier_buy_info_pk");

            entity.ToTable("article_supplier_buy_info");

            entity.HasIndex(e => e.CurrencyId, "IX_article_supplier_buy_info_currency_id");

            entity.HasIndex(e => e.ArticleId, "article_supplier_buy_info_article_id_index");

            entity.HasIndex(e => e.CreationDatetime, "article_supplier_buy_info_creation_datetime_index");

            entity.HasIndex(e => e.WhoProposed, "article_supplier_buy_info_who_proposed_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.CreationDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.CurrentSupplierStock).HasColumnName("current_supplier_stock");
            entity.Property(e => e.DeliveryIdDays).HasColumnName("delivery_id_days");
            entity.Property(e => e.WhoProposed).HasColumnName("who_proposed");

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleSupplierBuyInfos)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("article_supplier_buy_info_articles_id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.ArticleSupplierBuyInfos)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("article_supplier_buy_info_currency_id_fk");

            entity.HasOne(d => d.WhoProposedNavigation).WithMany(p => p.ArticleSupplierBuyInfos)
                .HasForeignKey(d => d.WhoProposed)
                .HasConstraintName("article_supplier_buy_info_users_id_fk");
        });

        modelBuilder.Entity<ArticleWeight>(entity =>
        {
            entity.HasKey(e => e.ArticleId).HasName("article_weight_pk");

            entity.ToTable("article_weight");

            entity.Property(e => e.ArticleId)
                .ValueGeneratedNever()
                .HasColumnName("article_id");
            entity.Property(e => e.Unit)
                .HasMaxLength(24)
                .HasColumnName("unit");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Article).WithOne(p => p.ArticleWeight)
                .HasForeignKey<ArticleWeight>(d => d.ArticleId)
                .HasConstraintName("article_weight_articles_id_fk");
        });

        modelBuilder.Entity<ArticlesContent>(entity =>
        {
            entity.HasKey(e => new { e.MainArticleId, e.InsideArticleId }).HasName("articles_content_pk");

            entity.ToTable("articles_content");

            entity.HasIndex(e => e.InsideArticleId, "article_main_inside_index");

            entity.HasIndex(e => e.MainArticleId, "articles_content_main_article_id_index");

            entity.Property(e => e.MainArticleId).HasColumnName("main_article_id");
            entity.Property(e => e.InsideArticleId).HasColumnName("inside_article_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.InsideArticle).WithMany(p => p.ArticlesContentInsideArticles)
                .HasForeignKey(d => d.InsideArticleId)
                .HasConstraintName("articles_content_in_id___fk");

            entity.HasOne(d => d.MainArticle).WithMany(p => p.ArticlesContentMainArticles)
                .HasForeignKey(d => d.MainArticleId)
                .HasConstraintName("articles_content_out_id___fk");
        });

        modelBuilder.Entity<ArticlesPair>(entity =>
        {
            entity.HasKey(e => new { e.ArticleLeft, e.ArticleRight }).HasName("articles_pair_pk");

            entity.ToTable("articles_pair");

            entity.HasIndex(e => e.ArticleRight, "IX_articles_pair_article_right");

            entity.HasIndex(e => e.ArticleLeft, "articles_pair_article_left_uindex").IsUnique();

            entity.Property(e => e.ArticleLeft).HasColumnName("article_left");
            entity.Property(e => e.ArticleRight).HasColumnName("article_right");

            entity.HasOne(d => d.ArticleLeftNavigation).WithOne(p => p.ArticlesPairArticleLeftNavigation)
                .HasForeignKey<ArticlesPair>(d => d.ArticleLeft)
                .HasConstraintName("articles_pair_articles_id_fk");

            entity.HasOne(d => d.ArticleRightNavigation).WithMany(p => p.ArticlesPairArticleRightNavigations)
                .HasForeignKey(d => d.ArticleRight)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("articles_pair_articles_id_fk_2");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ArticleId }).HasName("cart_pk");

            entity.ToTable("cart");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Article).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("cart_articles_id_fk");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("cart_users_id_fk");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pk");

            entity.ToTable("categories");

            entity.HasIndex(e => e.Name, "categories_name_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Coefficient>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("coefficients_pk");

            entity.ToTable("coefficients");

            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.Type)
                .HasMaxLength(56)
                .HasColumnName("type");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currency_pk");

            entity.ToTable("currency");

            entity.HasIndex(e => e.Code, "currency_code_uindex").IsUnique();

            entity.HasIndex(e => e.CurrencySign, "currency_currency_sign_uindex").IsUnique();

            entity.HasIndex(e => e.Name, "currency_name_uindex").IsUnique();

            entity.HasIndex(e => e.ShortName, "currency_short_name_uindex").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(26)
                .HasColumnName("code");
            entity.Property(e => e.CurrencySign)
                .HasMaxLength(3)
                .HasColumnName("currency_sign");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(5)
                .HasColumnName("short_name");
        });

        modelBuilder.Entity<CurrencyHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currency_history_pk");

            entity.ToTable("currency_history");

            entity.HasIndex(e => e.CurrencyId, "IX_currency_history_currency_id");

            entity.HasIndex(e => e.Datetime, "currency_history_datetime_index");

            entity.HasIndex(e => e.NewValue, "currency_history_new_value_index");

            entity.HasIndex(e => e.PrevValue, "currency_history_prev_value_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Datetime)
                .HasDefaultValueSql("now()")
                .HasColumnName("datetime");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.PrevValue).HasColumnName("prev_value");

            entity.HasOne(d => d.Currency).WithMany(p => p.CurrencyHistories)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("currency_history_currency_id_fk");
        });

        modelBuilder.Entity<CurrencyToUsd>(entity =>
        {
            entity.HasKey(e => e.CurrencyId).HasName("currency_to_usd_pk");

            entity.ToTable("currency_to_usd");

            entity.Property(e => e.CurrencyId)
                .ValueGeneratedNever()
                .HasColumnName("currency_id");
            entity.Property(e => e.ToUsd).HasColumnName("to_usd");

            entity.HasOne(d => d.Currency).WithOne(p => p.CurrencyToUsd)
                .HasForeignKey<CurrencyToUsd>(d => d.CurrencyId)
                .HasConstraintName("currency_to_usd_currency_id_fk");
        });

        modelBuilder.Entity<DefaultSetting>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("default_settings_pk");

            entity.ToTable("default_settings");

            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<MarkupGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("markup_group_pk");

            entity.ToTable("markup_group");

            entity.HasIndex(e => e.CurrencyId, "IX_markup_group_currency_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.IsAutoGenerated).HasColumnName("is_auto_generated");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Currency).WithMany(p => p.MarkupGroups)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("markup_group_currency_id_fk");
        });

        modelBuilder.Entity<MarkupRange>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("markup_ranges_pk");

            entity.ToTable("markup_ranges");

            entity.HasIndex(e => e.GroupId, "IX_markup_ranges_group_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Markup).HasColumnName("markup");
            entity.Property(e => e.RangeEnd).HasColumnName("range_end");
            entity.Property(e => e.RangeStart).HasColumnName("range_start");

            entity.HasOne(d => d.Group).WithMany(p => p.MarkupRanges)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("markup_ranges_markup_group_id_fk");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pk");

            entity.ToTable("orders");

            entity.HasIndex(e => e.BuyerApproved, "orders_buyer_approved_index");

            entity.HasIndex(e => e.CurrencyId, "orders_currency_id_index");

            entity.HasIndex(e => e.IsCanceled, "orders_is_canceled_index");

            entity.HasIndex(e => e.SellerApproved, "orders_seller_approved_index");

            entity.HasIndex(e => e.Status, "orders_status_index");

            entity.HasIndex(e => new { e.UserId, e.IsCanceled }, "orders_user_id_is_canceled_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BuyerApproved).HasColumnName("buyer_approved");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("create_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.IsCanceled).HasColumnName("is_canceled");
            entity.Property(e => e.SellerApproved).HasColumnName("seller_approved");
            entity.Property(e => e.SignedTotalPrice).HasColumnName("signed_total_price");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WhoUpdated).HasColumnName("who_updated");

            entity.HasOne(d => d.Currency).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_currency_id_fk");

            entity.HasOne(d => d.User).WithMany(p => p.OrderUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_users_id_fk");

            entity.HasOne(d => d.WhoUpdatedNavigation).WithMany(p => p.OrderWhoUpdatedNavigations)
                .HasForeignKey(d => d.WhoUpdated)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_users_id_fk_2");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_items_pk");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.ArticleId, "order_items_article_id_index");

            entity.HasIndex(e => e.OrderId, "order_items_order_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.LockedPrice).HasColumnName("locked_price");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.SignedPrice).HasColumnName("signed_price");

            entity.HasOne(d => d.Article).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_items_articles_id_fk");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_items_orders_id_fk");
        });

        modelBuilder.Entity<OrderVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_versions_pk");

            entity.ToTable("order_versions");

            entity.HasIndex(e => new { e.OrderId, e.Id }, "order_versions_order_id_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.BuyerApproved).HasColumnName("buyer_approved");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.SellerApproved).HasColumnName("seller_approved");
            entity.Property(e => e.SignedTotalPrice).HasColumnName("signed_total_price");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WhoUpdated).HasColumnName("who_updated");

            entity.HasOne(d => d.Currency).WithMany(p => p.OrderVersions)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_versions_currency_id_fk");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderVersions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_versions_orders_id_fk");

            entity.HasOne(d => d.WhoUpdatedNavigation).WithMany(p => p.OrderVersions)
                .HasForeignKey(d => d.WhoUpdated)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_versions_users_id_fk");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("permissions_pk");

            entity.ToTable("permissions", "auth");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Producer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("producer_id");

            entity.ToTable("producer");

            entity.HasIndex(e => e.Name, "producer_name_uindex").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.IsOe).HasColumnName("is_oe");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProducerDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("producer_details_pk");

            entity.ToTable("producer_details");

            entity.HasIndex(e => e.ProducerId, "producer_details_producer_id_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddressType)
                .HasMaxLength(32)
                .HasColumnName("address_type");
            entity.Property(e => e.City)
                .HasMaxLength(128)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(128)
                .HasColumnName("country");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(128)
                .HasColumnName("country_code");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.Name2)
                .HasMaxLength(128)
                .HasColumnName("name_2");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PostalCountryCode)
                .HasMaxLength(24)
                .HasColumnName("postal_country_code");
            entity.Property(e => e.ProducerId).HasColumnName("producer_id");
            entity.Property(e => e.Street)
                .HasMaxLength(64)
                .HasColumnName("street");
            entity.Property(e => e.Street2)
                .HasMaxLength(64)
                .HasColumnName("street_2");

            entity.HasOne(d => d.Producer).WithMany(p => p.ProducerDetails)
                .HasForeignKey(d => d.ProducerId)
                .HasConstraintName("producer_details_id_fk");
        });

        modelBuilder.Entity<ProducersOtherName>(entity =>
        {
            entity.HasKey(e => new { e.ProducerId, e.ProducerOtherName, e.WhereUsed }).HasName("producers_other_names_pk");

            entity.ToTable("producers_other_names");

            entity.HasIndex(e => e.ProducerId, "producers_other_names_producer_id_index");

            entity.HasIndex(e => e.ProducerOtherName, "producers_other_names_producer_other_name_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.WhereUsed, "producers_other_names_where_used_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.ProducerId).HasColumnName("producer_id");
            entity.Property(e => e.ProducerOtherName)
                .HasMaxLength(64)
                .HasColumnName("producer_other_name");
            entity.Property(e => e.WhereUsed)
                .HasMaxLength(64)
                .HasColumnName("where_used");

            entity.HasOne(d => d.Producer).WithMany(p => p.ProducersOtherNames)
                .HasForeignKey(d => d.ProducerId)
                .HasConstraintName("producers_other_names_producer_id_fk");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_pk");

            entity.ToTable("purchase");

            entity.HasIndex(e => e.Comment, "purchase_comment_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.CreatedUserId, "purchase_created_user_id_index");

            entity.HasIndex(e => e.CurrencyId, "purchase_currency_id_index");

            entity.HasIndex(e => e.PurchaseDatetime, "purchase_purchase_datetime_index");

            entity.HasIndex(e => e.State, "purchase_state_index");

            entity.HasIndex(e => e.Storage, "purchase_storage_index");

            entity.HasIndex(e => e.SupplierId, "purchase_supplier_id_index");

            entity.HasIndex(e => e.TransactionId, "purchase_transaction_id_index");

            entity.HasIndex(e => e.UpdatedUserId, "purchase_updated_user_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(256)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");
            entity.Property(e => e.CreationDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.PurchaseDatetime).HasColumnName("purchase_datetime");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Storage)
                .HasMaxLength(128)
                .HasColumnName("storage");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdateDatetime).HasColumnName("update_datetime");
            entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.PurchaseCreatedUsers)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_users_id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_currency_id_fk");

            entity.HasOne(d => d.StorageNavigation).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.Storage)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_storages_name_fk");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseSuppliers)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_users_id_fk_2");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_transactions_id_fk");

            entity.HasOne(d => d.UpdatedUser).WithMany(p => p.PurchaseUpdatedUsers)
                .HasForeignKey(d => d.UpdatedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_users_id_fk_3");
        });

        modelBuilder.Entity<PurchaseContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_content_pk");

            entity.ToTable("purchase_content");

            entity.HasIndex(e => e.ArticleId, "purchase_content_article_id_index");

            entity.HasIndex(e => e.Comment, "purchase_content_comment_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.PurchaseId, "purchase_content_purchase_id_index");

            entity.HasIndex(e => e.StorageContentId, "purchase_content_storage_content_id_uindex").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(256)
                .HasColumnName("comment");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.StorageContentId).HasColumnName("storage_content_id");
            entity.Property(e => e.TotalSum).HasColumnName("total_sum");

            entity.HasOne(d => d.Article).WithMany(p => p.PurchaseContents)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_content_articles_id_fk");

            entity.HasOne(d => d.Purchase).WithMany(p => p.PurchaseContents)
                .HasForeignKey(d => d.PurchaseId)
                .HasConstraintName("purchase_content_purchase_id_fk");

            entity.HasOne(d => d.StorageContent).WithOne(p => p.PurchaseContent)
                .HasForeignKey<PurchaseContent>(d => d.StorageContentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("purchase_content_storage_content_id_fk");
        });

        modelBuilder.Entity<PurchaseContentLogistic>(entity =>
        {
            entity.HasKey(e => e.PurchaseContentId).HasName("purchase_content_logistics_pk");

            entity.ToTable("purchase_content_logistics");

            entity.Property(e => e.PurchaseContentId)
                .ValueGeneratedNever()
                .HasColumnName("purchase_content_id");
            entity.Property(e => e.AreaM3).HasColumnName("area_m3");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.WeightKg).HasColumnName("weight_kg");

            entity.HasOne(d => d.PurchaseContent).WithOne(p => p.PurchaseContentLogistic)
                .HasForeignKey<PurchaseContentLogistic>(d => d.PurchaseContentId)
                .HasConstraintName("purchase_content_logistics_purchase_content_id_fk");
        });

        modelBuilder.Entity<PurchaseLogistic>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("purchase_logistics_pk");

            entity.ToTable("purchase_logistics");

            entity.HasIndex(e => e.CurrencyId, "purchase_logistics_currency_id_index");

            entity.HasIndex(e => e.RouteId, "purchase_logistics_route_id_index");

            entity.HasIndex(e => e.TransactionId, "purchase_logistics_transaction_id_uindex").IsUnique();

            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.MinimumPrice).HasColumnName("minimum_price");
            entity.Property(e => e.MinimumPriceApplied).HasColumnName("minimum_price_applied");
            entity.Property(e => e.PriceKg).HasColumnName("price_kg");
            entity.Property(e => e.PricePerM3).HasColumnName("price_per_m3");
            entity.Property(e => e.PricePerOrder).HasColumnName("price_per_order");
            entity.Property(e => e.PricingModel)
                .HasMaxLength(24)
                .HasColumnName("pricing_model");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.RouteType)
                .HasMaxLength(24)
                .HasColumnName("route_type");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Currency).WithMany(p => p.PurchaseLogistics)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_logistics_currency_id_fk");

            entity.HasOne(d => d.Purchase).WithOne(p => p.PurchaseLogistic)
                .HasForeignKey<PurchaseLogistic>(d => d.PurchaseId)
                .HasConstraintName("purchase_logistics_purchase_id_fk");

            entity.HasOne(d => d.Route).WithMany(p => p.PurchaseLogistics)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_logistics_storage_routes_id_fk");

            entity.HasOne(d => d.Transaction).WithOne(p => p.PurchaseLogistic)
                .HasForeignKey<PurchaseLogistic>(d => d.TransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_logistics_transactions_id_fk");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pk");

            entity.ToTable("roles", "auth");

            entity.HasIndex(e => e.NormalizedName, "roles_normalized_name_uindex").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsSystem).HasColumnName("is_system");
            entity.Property(e => e.Name)
                .HasMaxLength(24)
                .HasColumnName("name");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(24)
                .HasColumnName("normalized_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasMany(d => d.PermissionNames).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionName")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("role_permissions_permissions_name_fk"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("role_permissions_roles_id_fk"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionName").HasName("role_permissions_pk");
                        j.ToTable("role_permissions", "auth");
                        j.HasIndex(new[] { "PermissionName" }, "IX_role_permissions_permission_name");
                        j.IndexerProperty<Guid>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<string>("PermissionName").HasColumnName("permission_name");
                    });
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_pk");

            entity.ToTable("sale");

            entity.HasIndex(e => e.BuyerId, "sale_buyer_id_index");

            entity.HasIndex(e => e.Comment, "sale_comment_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

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
                .HasOperators(new[] { "gin_trgm_ops" });

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

            entity.HasOne(d => d.Article).WithMany(p => p.SaleContents)
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
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Location, "storages_location_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

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
                .HasOperators(new[] { "gin_trgm_ops" });

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

            entity.HasOne(d => d.Article).WithMany(p => p.StorageContents)
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

            entity.HasIndex(e => new { e.ArticleId, e.IsDone }, "storage_content_reservations_article_id_is_done_index");

            entity.HasIndex(e => e.Comment, "storage_content_reservations_comment_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

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

            entity.HasOne(d => d.Article).WithMany(p => p.StorageContentReservations)
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

            entity.HasOne(d => d.Article).WithMany(p => p.StorageMovements)
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

            entity.HasIndex(e => new { e.FromStorageName, e.ToStorageName, e.IsActive }, "storage_from_to_active_uindex")
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

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transactions_pk");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.CurrencyId, "IX_transactions_currency_id");

            entity.HasIndex(e => e.CreationDate, "transactions_creation_date_index");

            entity.HasIndex(e => e.DeletedBy, "transactions_deleted_by_index");

            entity.HasIndex(e => e.IsDeleted, "transactions_is_deleted_index");

            entity.HasIndex(e => e.ReceiverId, "transactions_receiver_id_index");

            entity.HasIndex(e => e.SenderId, "transactions_sender_id_index");

            entity.HasIndex(e => new { e.SenderId, e.ReceiverId }, "transactions_sender_id_receiver_id_index");

            entity.HasIndex(e => e.Status, "transactions_status_index");

            entity.HasIndex(e => new { e.TransactionDatetime, e.Id }, "transactions_transaction_datetime_id_index");

            entity.HasIndex(e => e.TransactionDatetime, "transactions_transaction_datetime_index");

            entity.HasIndex(e => e.TransactionDatetime, "transactions_transaction_datetime_sender_id_receiver_id_idx").IsDescending();

            entity.HasIndex(e => e.WhoMadeUserId, "transactions_who_made_user_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.ReceiverBalanceAfterTransaction).HasColumnName("receiver_balance_after_transaction");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderBalanceAfterTransaction).HasColumnName("sender_balance_after_transaction");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Status)
                .HasMaxLength(28)
                .HasColumnName("status");
            entity.Property(e => e.TransactionDatetime).HasColumnName("transaction_datetime");
            entity.Property(e => e.TransactionSum).HasColumnName("transaction_sum");
            entity.Property(e => e.WhoMadeUserId).HasColumnName("who_made_user_id");

            entity.HasOne(d => d.Currency).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_currency_id_fk");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.TransactionDeletedByNavigations)
                .HasForeignKey(d => d.DeletedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_users_id_fk_4");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TransactionReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_users_id_fk_2");

            entity.HasOne(d => d.Sender).WithMany(p => p.TransactionSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_users_id_fk");

            entity.HasOne(d => d.WhoMadeUser).WithMany(p => p.TransactionWhoMadeUsers)
                .HasForeignKey(d => d.WhoMadeUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_users_id_fk_3");

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<TransactionVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transaction_versions_pk");

            entity.ToTable("transaction_versions");

            entity.HasIndex(e => e.CurrencyId, "transaction_versions_currency_id_index");

            entity.HasIndex(e => e.ReceiverId, "transaction_versions_receiver_id_index");

            entity.HasIndex(e => e.SenderId, "transaction_versions_sender_id _index");

            entity.HasIndex(e => e.Status, "transaction_versions_status_index");

            entity.HasIndex(e => e.TransactionDatetime, "transaction_versions_transaction_datetime_index");

            entity.HasIndex(e => e.TransactionId, "transaction_versions_transaction_id_index");

            entity.HasIndex(e => new { e.TransactionId, e.Version }, "transaction_versions_transaction_id_version_uindex").IsUnique();

            entity.HasIndex(e => e.VersionCreatedDatetime, "transaction_versions_version_created_datetime_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id ");
            entity.Property(e => e.Status)
                .HasMaxLength(28)
                .HasColumnName("status");
            entity.Property(e => e.TransactionDatetime).HasColumnName("transaction_datetime");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.TransactionSum).HasColumnName("transaction_sum");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.VersionCreatedDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnName("version_created_datetime");

            entity.HasOne(d => d.Currency).WithMany(p => p.TransactionVersions)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transaction_versions_currency_id_fk");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TransactionVersionReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transaction_versions_users_id_fk");

            entity.HasOne(d => d.Sender).WithMany(p => p.TransactionVersionSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transaction_versions_users_id_fk_2");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionVersions)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transaction_versions_transactions_id_fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pk");

            entity.ToTable("users", "auth");

            entity.HasIndex(e => e.NormalizedUserName, "users_normalized_user_name_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.NormalizedUserName, "users_normalized_user_name_uindex").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AccessFailedCount).HasColumnName("access_failed_count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.LockoutEnd).HasColumnName("lockout_end");
            entity.Property(e => e.NormalizedUserName)
                .HasMaxLength(36)
                .HasColumnName("normalized_user_name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserName)
                .HasMaxLength(36)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<UserBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_balances_pk");

            entity.ToTable("user_balances");

            entity.HasIndex(e => e.Balance, "user_balances_balance_index");

            entity.HasIndex(e => e.CurrencyId, "user_balances_currency_id_index");

            entity.HasIndex(e => new { e.CurrencyId, e.UserId }, "user_balances_currency_id_user_id_uindex").IsUnique();

            entity.HasIndex(e => e.UserId, "user_balances_user_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('table_name_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Currency).WithMany(p => p.UserBalances)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_balances_currency_id_fk");

            entity.HasOne(d => d.User).WithMany(p => p.UserBalances)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_balances_users_id_fk");
        });

        modelBuilder.Entity<UserDiscount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_discounts_pk");

            entity.ToTable("user_discounts");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Discount).HasColumnName("discount");

            entity.HasOne(d => d.User).WithOne(p => p.UserDiscount)
                .HasForeignKey<UserDiscount>(d => d.UserId)
                .HasConstraintName("user_discounts_users_id_fk");
        });

        modelBuilder.Entity<UserEmail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_emails_pk");

            entity.ToTable("user_emails", "auth");

            entity.HasIndex(e => e.NormalizedEmail, "user_emails_normalized_email_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.NormalizedEmail, "user_emails_normalized_email_uindex").IsUnique();

            entity.HasIndex(e => e.UserId, "user_emails_user_id_index");

            entity.HasIndex(e => new { e.UserId, e.IsPrimary }, "user_emails_user_id_is_primary_uindex")
                .IsUnique()
                .HasFilter("(is_primary = true)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");
            entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailType)
                .HasMaxLength(50)
                .HasColumnName("email_type");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.NormalizedEmail)
                .HasMaxLength(255)
                .HasColumnName("normalized_email");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserEmails)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_emails_users_id_fk");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_info_pk");

            entity.ToTable("user_info", "auth");

            entity.HasIndex(e => e.Description, "user_info_description_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.IsSupplier, "user_info_is_supplier_index");

            entity.HasIndex(e => e.Name, "user_info_name_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.SearchColumn, "user_info_search_column_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Surname, "user_info_surname_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsSupplier).HasColumnName("is_supplier");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.SearchColumn).HasColumnName("search_column");
            entity.Property(e => e.Surname).HasColumnName("surname");

            entity.HasOne(d => d.User).WithOne(p => p.UserInfo)
                .HasForeignKey<UserInfo>(d => d.UserId)
                .HasConstraintName("user_info_users_id_fk");
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.Permission }).HasName("user_permissions_pk");

            entity.ToTable("user_permissions", "auth");

            entity.HasIndex(e => e.Permission, "IX_user_permissions_permission");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Permission).HasColumnName("permission");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.HasOne(d => d.PermissionNavigation).WithMany(p => p.UserPermissions)
                .HasForeignKey(d => d.Permission)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_permissions_permissions_name_fk");

            entity.HasOne(d => d.User).WithMany(p => p.UserPermissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_permissions_users_id_fk");
        });

        modelBuilder.Entity<UserPhone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_phones_pk");

            entity.ToTable("user_phones", "auth");

            entity.HasIndex(e => e.NormalizedPhone, "user_phones_normalized_phone_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.NormalizedPhone, "user_phones_normalized_phone_uindex").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.IsPrimary }, "user_phones_user_id_is_primary_uindex")
                .IsUnique()
                .HasFilter("(is_primary = true)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");
            entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.NormalizedPhone)
                .HasMaxLength(32)
                .HasColumnName("normalized_phone");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(32)
                .HasColumnName("phone_number");
            entity.Property(e => e.PhoneType)
                .HasMaxLength(32)
                .HasColumnName("phone_type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserPhones)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_phones_user_id_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("user_roles_pk");

            entity.ToTable("user_roles", "auth");

            entity.HasIndex(e => e.RoleId, "IX_user_roles_role_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("assigned_at");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("user_roles_roles_id_fk");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_roles_users_id_fk");
        });

        modelBuilder.Entity<UserSearchHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_search_history_pk");

            entity.ToTable("user_search_history");

            entity.HasIndex(e => e.SearchDateTime, "user_search_history_search_date_time_index");

            entity.HasIndex(e => e.SearchPlace, "user_search_history_search_place_index");

            entity.HasIndex(e => e.UserId, "user_search_history_user_id_index");

            entity.HasIndex(e => new { e.UserId, e.SearchPlace }, "user_search_history_user_id_search_place_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Query)
                .HasColumnType("jsonb")
                .HasColumnName("query");
            entity.Property(e => e.SearchDateTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("search_date_time");
            entity.Property(e => e.SearchPlace).HasColumnName("search_place");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserSearchHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_search_history_users_id_fk");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_tokens_pk");

            entity.ToTable("user_tokens", "auth");

            entity.HasIndex(e => e.ExpiresAt, "user_tokens_expires_at_index").HasFilter("((revoked = false) AND (expires_at IS NOT NULL))");

            entity.HasIndex(e => e.Permissions, "user_tokens_permissions_index").HasMethod("gin");

            entity.HasIndex(e => e.TokenHash, "user_tokens_token_hash_uindex").IsUnique();

            entity.HasIndex(e => e.UserId, "user_tokens_user_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(255)
                .HasColumnName("device_id");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("issued_at");
            entity.Property(e => e.Permissions).HasColumnName("permissions");
            entity.Property(e => e.RevokeReason)
                .HasMaxLength(255)
                .HasColumnName("revoke_reason");
            entity.Property(e => e.Revoked).HasColumnName("revoked");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_tokens_users_id_fk");
        });

        modelBuilder.Entity<UserVehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_vehicles_pk");

            entity.ToTable("user_vehicles");

            entity.HasIndex(e => e.Comment, "user_vehicles_comment_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Manufacture, "user_vehicles_manufacture_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Model, "user_vehicles_model_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.PlateNumber, "user_vehicles_plate_number_uindex").IsUnique();

            entity.HasIndex(e => e.UserId, "user_vehicles_user_id_index");

            entity.HasIndex(e => e.Vin, "user_vehicles_vin_uindex").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EngineCode).HasColumnName("engine_code");
            entity.Property(e => e.Manufacture)
                .HasMaxLength(50)
                .HasColumnName("manufacture");
            entity.Property(e => e.Model)
                .HasMaxLength(125)
                .HasColumnName("model");
            entity.Property(e => e.Modification).HasColumnName("modification");
            entity.Property(e => e.PlateNumber).HasColumnName("plate_number");
            entity.Property(e => e.ProductionYear).HasColumnName("production_year");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Vin)
                .HasMaxLength(50)
                .HasColumnName("vin");

            entity.HasOne(d => d.User).WithMany(p => p.UserVehicles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_vehicles_users_id_fk");
        });
        modelBuilder.HasSequence<int>("storage_movement_id_seq");
        modelBuilder.HasSequence<int>("table_name_id_seq");

        modelBuilder.AllDateTimesToUtc()
            .AllEnumsToString();
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MonoliteUnicorn.PostGres.Main;

public partial class DContext : DbContext
{
    public DContext()
    {
    }

    public DContext(DbContextOptions<DContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<ArticleCharacteristic> ArticleCharacteristics { get; set; }

    public virtual DbSet<ArticleCross> ArticleCrosses { get; set; }

    public virtual DbSet<ArticleEan> ArticleEans { get; set; }

    public virtual DbSet<ArticleImage> ArticleImages { get; set; }

    public virtual DbSet<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; }

    public virtual DbSet<ArticlesContent> ArticlesContents { get; set; }

    public virtual DbSet<ArticlesPair> ArticlesPairs { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<BuySellPrice> BuySellPrices { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<CurrencyHistory> CurrencyHistories { get; set; }

    public virtual DbSet<CurrencyToUsd> CurrencyToUsds { get; set; }

    public virtual DbSet<DefaultSetting> DefaultSettings { get; set; }

    public virtual DbSet<MarkupGroup> MarkupGroups { get; set; }

    public virtual DbSet<MarkupRange> MarkupRanges { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<ProducerDetail> ProducerDetails { get; set; }

    public virtual DbSet<ProducersOtherName> ProducersOtherNames { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<PurchaseContent> PurchaseContents { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleContent> SaleContents { get; set; }

    public virtual DbSet<SaleContentDetail> SaleContentDetails { get; set; }

    public virtual DbSet<Storage> Storages { get; set; }

    public virtual DbSet<StorageContent> StorageContents { get; set; }

    public virtual DbSet<StorageMovement> StorageMovements { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionVersion> TransactionVersions { get; set; }

    public virtual DbSet<UserBalance> UserBalances { get; set; }

    public virtual DbSet<UserDiscount> UserDiscounts { get; set; }

    public virtual DbSet<UserMail> UserMails { get; set; }

    public virtual DbSet<UserVehicle> UserVehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=mono_unicorn;Username=postgres;Password=PleasKillMe21");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("car_types", new[] { "PassengerCar", "CommercialVehicle", "Motorbike" })
            .HasPostgresExtension("dblink")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("articles_id_pk");

            entity.ToTable("articles");

            entity.HasIndex(e => e.ArticleName, "articles_article_name_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.ArticleNumber, "articles_article_number_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.CategoryId, "articles_category_id_index");

            entity.HasIndex(e => e.ProducerId, "articles_producer_id_index");

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
            entity.Property(e => e.IsOe)
                .HasDefaultValue(false)
                .HasColumnName("is_oe");
            entity.Property(e => e.IsValid)
                .HasDefaultValue(true)
                .HasColumnName("is_valid");
            entity.Property(e => e.NormalizedArticleNumber)
                .HasMaxLength(128)
                .HasColumnName("normalized_article_number");
            entity.Property(e => e.PackingUnit).HasColumnName("packing_unit");
            entity.Property(e => e.ProducerId).HasColumnName("producer_id");
            entity.Property(e => e.TotalCount)
                .HasDefaultValue(0)
                .HasColumnName("total_count");

            entity.HasOne(d => d.Category).WithMany(p => p.Articles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("articles_categories_id_fk");

            entity.HasOne(d => d.Producer).WithMany(p => p.Articles)
                .HasForeignKey(d => d.ProducerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("producer_id_fk");
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

        modelBuilder.Entity<ArticleCross>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("article_crosses");

            entity.HasIndex(e => new { e.ArticleCrossId, e.ArticleId }, "article_crosses_article_cross_id_article_id_uindex").IsUnique();

            entity.HasIndex(e => e.ArticleCrossId, "article_crosses_article_cross_id_index");

            entity.HasIndex(e => e.ArticleId, "article_crosses_article_id_index");

            entity.Property(e => e.ArticleCrossId).HasColumnName("article_cross_id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
        });

        modelBuilder.Entity<ArticleEan>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("article_ean");

            entity.HasIndex(e => e.Ean, "article_ean_ean_index");

            entity.HasIndex(e => e.ArticleId, "article_ean_id__index");

            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Ean)
                .HasMaxLength(24)
                .HasColumnName("ean");

            entity.HasOne(d => d.Article).WithMany()
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

        modelBuilder.Entity<ArticleSupplierBuyInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("article_supplier_buy_info_pk");

            entity.ToTable("article_supplier_buy_info");

            entity.HasIndex(e => e.ArticleId, "article_supplier_buy_info_article_id_index");

            entity.HasIndex(e => e.CreationDatetime, "article_supplier_buy_info_creation_datetime_index");

            entity.HasIndex(e => e.WhoProposed, "article_supplier_buy_info_who_proposed_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.CreationDatetime)
                .HasDefaultValueSql("(now())::timestamp without time zone")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creation_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.CurrentSupplierStock)
                .HasDefaultValue(0)
                .HasColumnName("current_supplier_stock");
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
                .HasConstraintName("article_supplier_buy_info_aspnetusers_id_fk");
        });

        modelBuilder.Entity<ArticlesContent>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("articles_content");

            entity.HasIndex(e => e.InsideArticleId, "article_main_inside_index");

            entity.HasIndex(e => e.MainArticleId, "articles_content_main_article_id_index");

            entity.HasIndex(e => new { e.MainArticleId, e.InsideArticleId }, "articles_content_main_article_id_inside_article_id_uindex").IsUnique();

            entity.Property(e => e.InsideArticleId).HasColumnName("inside_article_id");
            entity.Property(e => e.MainArticleId).HasColumnName("main_article_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");

            entity.HasOne(d => d.InsideArticle).WithMany()
                .HasForeignKey(d => d.InsideArticleId)
                .HasConstraintName("articles_content_in_id___fk");

            entity.HasOne(d => d.MainArticle).WithMany()
                .HasForeignKey(d => d.MainArticleId)
                .HasConstraintName("articles_content_out_id___fk");
        });

        modelBuilder.Entity<ArticlesPair>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("articles_pair");

            entity.HasIndex(e => e.ArticleLeft, "articles_pair_article_left_index");

            entity.HasIndex(e => e.ArticleRight, "articles_pair_article_right_index");

            entity.Property(e => e.ArticleLeft).HasColumnName("article_left");
            entity.Property(e => e.ArticleRight).HasColumnName("article_right");

            entity.HasOne(d => d.ArticleLeftNavigation).WithMany()
                .HasForeignKey(d => d.ArticleLeft)
                .HasConstraintName("articles_pair_articles_id_fk");

            entity.HasOne(d => d.ArticleRightNavigation).WithMany()
                .HasForeignKey(d => d.ArticleRight)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("articles_pair_articles_id_fk_2");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.IsSupplier).HasDefaultValue(false);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<BuySellPrice>(entity =>
        {
            entity.HasKey(e => e.SaleContentId).HasName("buy_sell_prices_pk");

            entity.ToTable("buy_sell_prices");

            entity.HasIndex(e => e.ArticleId, "buy_sell_prices_article_id_index");

            entity.HasIndex(e => e.BuyPrice, "buy_sell_prices_buy_price_index");

            entity.HasIndex(e => e.SellPrice, "buy_sell_prices_sell_price_index");

            entity.Property(e => e.SaleContentId)
                .ValueGeneratedNever()
                .HasColumnName("sale_content_id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.IsOutLiner)
                .HasDefaultValue(false)
                .HasColumnName("is_out_liner");
            entity.Property(e => e.Markup).HasColumnName("markup");
            entity.Property(e => e.SellPrice).HasColumnName("sell_price");

            entity.HasOne(d => d.Article).WithMany(p => p.BuySellPrices)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("buy_sell_prices_articles_id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.BuySellPrices)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("buy_sell_prices_currency_id_fk");

            entity.HasOne(d => d.SaleContent).WithOne(p => p.BuySellPrice)
                .HasForeignKey<BuySellPrice>(d => d.SaleContentId)
                .HasConstraintName("buy_sell_prices_sale_content_id_fk");
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

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currency_pk");

            entity.ToTable("currency");

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

            entity.HasIndex(e => e.Datetime, "currency_history_datetime_index");

            entity.HasIndex(e => e.NewValue, "currency_history_new_value_index");

            entity.HasIndex(e => e.PrevValue, "currency_history_prev_value_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Datetime)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
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

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.IsAutoGenerated)
                .HasDefaultValue(false)
                .HasColumnName("is_auto_generated");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Currency).WithMany(p => p.MarkupGroups)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("markup_group_currency_id_fk");
        });

        modelBuilder.Entity<MarkupRange>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("markup_ranges_pk");

            entity.ToTable("markup_ranges");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Markup).HasColumnName("markup");
            entity.Property(e => e.RangeEnd).HasColumnName("range_end");
            entity.Property(e => e.RangeStart).HasColumnName("range_start");

            entity.HasOne(d => d.Group).WithMany(p => p.MarkupRanges)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("markup_ranges_markup_group_id_fk");
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
            entity.Property(e => e.IsOe)
                .HasDefaultValue(false)
                .HasColumnName("is_oe");
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
            entity
                .HasNoKey()
                .ToTable("producers_other_names");

            entity.HasIndex(e => new { e.ProducerOtherName, e.ProducerId, e.WhereUsed }, "full_uniq_producer_other_name").IsUnique();

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

            entity.HasOne(d => d.Producer).WithMany()
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
                .HasDefaultValueSql("(now())::timestamp without time zone")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creation_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.PurchaseDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("purchase_datetime");
            entity.Property(e => e.Storage).HasColumnName("storage");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdateDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("update_datetime");
            entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.PurchaseCreatedUsers)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_aspnetusers_id_fk");

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
                .HasConstraintName("purchase_aspnetusers_id_fk_2");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_transactions_id_fk");

            entity.HasOne(d => d.UpdatedUser).WithMany(p => p.PurchaseUpdatedUsers)
                .HasForeignKey(d => d.UpdatedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_aspnetusers_id_fk_3");
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

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(256)
                .HasColumnName("comment");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.TotalSum).HasColumnName("total_sum");

            entity.HasOne(d => d.Article).WithMany(p => p.PurchaseContents)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("purchase_content_articles_id_fk");

            entity.HasOne(d => d.Purchase).WithMany(p => p.PurchaseContents)
                .HasForeignKey(d => d.PurchaseId)
                .HasConstraintName("purchase_content_purchase_id_fk");
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
                .HasDefaultValueSql("(now())::timestamp without time zone")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creation_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.MainStorageName).HasColumnName("main_storage_name");
            entity.Property(e => e.SaleDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sale_datetime");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdateDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("update_datetime");
            entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

            entity.HasOne(d => d.Buyer).WithMany(p => p.SaleBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_aspnetusers_id_fk_2");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.SaleCreatedUsers)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sale_aspnetusers_id_fk");

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
                .HasConstraintName("sale_aspnetusers_id_fk_3");
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

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.PurchaseDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("purchase_datetime");
            entity.Property(e => e.SaleContentId).HasColumnName("sale_content_id");
            entity.Property(e => e.Storage).HasColumnName("storage");
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

            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(256)
                .HasColumnName("location");
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

            entity.HasIndex(e => e.Status, "storage_content_status_index");

            entity.HasIndex(e => e.StorageName, "storage_content_storage_name_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.BuyPriceInUsd).HasColumnName("buy_price_in_usd");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("(now())::timestamp without time zone")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_datetime");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.PurchaseDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("purchase_datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(26)
                .HasDefaultValueSql("'Ok'::character varying")
                .HasColumnName("status");
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

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(24)
                .HasColumnName("action_type");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.StorageName).HasColumnName("storage_name");
            entity.Property(e => e.WhoMoved).HasColumnName("who_moved");

            entity.HasOne(d => d.Article).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("storage_movement_articles_id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("storage_movement_currency_id_fk");

            entity.HasOne(d => d.StorageNameNavigation).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.StorageName)
                .HasConstraintName("storage_movement_storages_name_fk");

            entity.HasOne(d => d.WhoMovedNavigation).WithMany(p => p.StorageMovements)
                .HasForeignKey(d => d.WhoMoved)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("storage_movement_aspnetusers_id_fk");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transactions_pk");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.CreationDate, "transactions_creation_date_index");

            entity.HasIndex(e => e.DeletedBy, "transactions_deleted_by_index");

            entity.HasIndex(e => e.IsDeleted, "transactions_is_deleted_index");

            entity.HasIndex(e => e.ReceiverId, "transactions_receiver_id_index");

            entity.HasIndex(e => new { e.ReceiverId, e.TransactionDatetime, e.SenderId }, "transactions_receiver_id_transaction_datetime_sender_id_uindex").IsUnique();

            entity.HasIndex(e => e.SenderId, "transactions_sender_id_index");

            entity.HasIndex(e => new { e.SenderId, e.ReceiverId }, "transactions_sender_id_receiver_id_index");

            entity.HasIndex(e => e.Status, "transactions_status_index");

            entity.HasIndex(e => new { e.TransactionDatetime, e.Id }, "transactions_transaction_datetime_id_index");

            entity.HasIndex(e => e.TransactionDatetime, "transactions_transaction_datetime_index");

            entity.HasIndex(e => e.WhoMadeUserId, "transactions_who_made_user_id_index");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(now())::timestamp without time zone")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creation_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.ReceiverBalanceAfterTransaction).HasColumnName("receiver_balance_after_transaction");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderBalanceAfterTransaction).HasColumnName("sender_balance_after_transaction");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Status)
                .HasMaxLength(28)
                .HasColumnName("status");
            entity.Property(e => e.TransactionDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_datetime");
            entity.Property(e => e.TransactionSum).HasColumnName("transaction_sum");
            entity.Property(e => e.WhoMadeUserId).HasColumnName("who_made_user_id");

            entity.HasOne(d => d.Currency).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_currency_id_fk");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.TransactionDeletedByNavigations)
                .HasForeignKey(d => d.DeletedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_aspnetusers_id_fk_4");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TransactionReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_aspnetusers_id_fk_2");

            entity.HasOne(d => d.Sender).WithMany(p => p.TransactionSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_aspnetusers_id_fk");

            entity.HasOne(d => d.WhoMadeUser).WithMany(p => p.TransactionWhoMadeUsers)
                .HasForeignKey(d => d.WhoMadeUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transactions_aspnetusers_id_fk_3");
            
            entity.HasQueryFilter(t => !t.IsDeleted);
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
            entity.Property(e => e.TransactionDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_datetime");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.TransactionSum).HasColumnName("transaction_sum");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.VersionCreatedDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("version_created_datetime");

            entity.HasOne(d => d.Currency).WithMany(p => p.TransactionVersions)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transaction_versions_currency_id_fk");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TransactionVersionReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transaction_versions_aspnetusers_id_fk");

            entity.HasOne(d => d.Sender).WithMany(p => p.TransactionVersionSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transaction_versions_aspnetusers_id_fk_2");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionVersions)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("transaction_versions_transactions_id_fk");
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
                .HasConstraintName("table_name_aspnetusers_id_fk");
        });

        modelBuilder.Entity<UserDiscount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_discounts_pk");

            entity.ToTable("user_discounts");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Discount).HasColumnName("discount");

            entity.HasOne(d => d.User).WithOne(p => p.UserDiscount)
                .HasForeignKey<UserDiscount>(d => d.UserId)
                .HasConstraintName("user_discounts_aspnetusers_id_fk");
        });

        modelBuilder.Entity<UserMail>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("user_mails_pk");

            entity.ToTable("user_mails");

            entity.HasIndex(e => e.NormalizedEmail, "user_mails_normalized_email_index")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.NormalizedEmail, "user_mails_normalized_email_uindex").IsUnique();

            entity.HasIndex(e => e.UserId, "user_mails_user_id_index");

            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
            entity.Property(e => e.NormalizedEmail).HasColumnName("normalized_email");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserMails)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_mails_aspnetusers_id_fk");
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
                .HasDefaultValueSql("(now())::timestamp without time zone")
                .HasColumnType("timestamp without time zone")
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
                .HasConstraintName("user_vehicles_aspnetusers_id_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using Analytics.Entities;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Persistence.Context;

public partial class DContext : DbContext
{
    public DContext()
    {
    }

    public DContext(DbContextOptions<DContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<PurchaseContent> PurchaseContents { get; set; }

    public virtual DbSet<PurchasesFact> PurchasesFacts { get; set; }

    public virtual DbSet<SellInfo> SellInfos { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();

        modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", "msg");
        modelBuilder.Entity<OutboxState>().ToTable("OutboxState", "msg");
        modelBuilder.Entity<InboxState>().ToTable("InboxState", "msg");
        
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currencies_pk");

            entity.ToTable("currencies");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ToUsd).HasColumnName("to_usd");
        });

        modelBuilder.Entity<PurchaseContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchase_contents_pk");

            entity.ToTable("purchase_contents");

            entity.HasIndex(e => e.ArticleId, "purchase_contents_article_id_index");

            entity.HasIndex(e => e.PurchaseId, "purchase_contents_purchase_id_index");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");

            entity.HasOne(d => d.Purchase).WithMany(p => p.PurchaseContents)
                .HasForeignKey(d => d.PurchaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchase_contents_purchases_fact_id_fk");
        });

        modelBuilder.Entity<PurchasesFact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purchases_fact_pk");

            entity.ToTable("purchases_fact");

            entity.HasIndex(e => e.CreatedAt, "purchases_fact_created_at_index");

            entity.HasIndex(e => e.CurrencyId, "purchases_fact_currency_id_index");

            entity.HasIndex(e => e.SupplierId, "purchases_fact_supplier_id_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount");

            entity.HasOne(d => d.Currency).WithMany(p => p.PurchasesFacts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchases_fact_currencies_id_fk");
        });

        modelBuilder.Entity<SellInfo>(entity =>
        {
            entity.HasKey(e => e.SellContentId).HasName("sell_info_pk");

            entity.ToTable("sell_info");

            entity.HasIndex(e => e.ArticleId, "sell_info_article_id_index");

            entity.HasIndex(e => new { e.ArticleId, e.StorageName }, "sell_info_article_id_storage_name_index");

            entity.HasIndex(e => e.BuyCurrencyId, "sell_info_buy_currency_id_index");

            entity.HasIndex(e => e.BuyPrices, "sell_info_buy_prices_index");

            entity.HasIndex(e => e.Markup, "sell_info_markup_index");

            entity.HasIndex(e => e.SellCurrencyId, "sell_info_sell_currency_id_index");

            entity.HasIndex(e => e.SellDate, "sell_info_sell_date_index");

            entity.HasIndex(e => e.SellPrice, "sell_info_sell_price_index");

            entity.HasIndex(e => e.StorageName, "sell_info_storage_name_index");

            entity.Property(e => e.SellContentId).HasColumnName("sell_content_id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.BuyCurrencyId).HasColumnName("buy_currency_id");
            entity.Property(e => e.BuyPrices).HasColumnName("buy_prices");
            entity.Property(e => e.Markup).HasColumnName("markup");
            entity.Property(e => e.SellCurrencyId).HasColumnName("sell_currency_id");
            entity.Property(e => e.SellDate).HasColumnName("sell_date");
            entity.Property(e => e.SellPrice).HasColumnName("sell_price");
            entity.Property(e => e.StorageName).HasColumnName("storage_name");

            entity.HasOne(d => d.BuyCurrency).WithMany(p => p.SellInfoBuyCurrencies)
                .HasForeignKey(d => d.BuyCurrencyId)
                .HasConstraintName("sell_info_currencies_id_fk");

            entity.HasOne(d => d.SellCurrency).WithMany(p => p.SellInfoSellCurrencies)
                .HasForeignKey(d => d.SellCurrencyId)
                .HasConstraintName("sell_info_currencies_id_fk_2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

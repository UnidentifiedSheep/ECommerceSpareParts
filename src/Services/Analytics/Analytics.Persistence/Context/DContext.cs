using System.Text.Json;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

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

    public virtual DbSet<Metric> Metrics { get; set; }

    public virtual DbSet<PurchaseContent> PurchaseContents { get; set; }

    public virtual DbSet<PurchasesFact> PurchasesFacts { get; set; }

    public virtual DbSet<SaleContent> SaleContents { get; set; }

    public virtual DbSet<SaleContentDetail> SaleContentDetails { get; set; }

    public virtual DbSet<SalesFact> SalesFacts { get; set; }
    
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

        modelBuilder.Entity<ArticleSalesMetric>(entity =>
        {
            entity.HasIndex(e => new { e.Discriminator, e.ArticleId }, 
                "metrics_discriminator_article_index");
            
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
        });
        
        modelBuilder.Entity<Metric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("metrics_pk");

            entity.ToTable("metrics");

            entity.HasIndex(e => e.CreatedAt, "metrics_created_at_index");

            entity.HasIndex(e => e.CreatedBy, "metrics_created_by_index");

            entity.HasIndex(e => e.CurrencyId, "metrics_currency_id_index");

            entity.HasIndex(e => e.Discriminator, "metrics_dirty_index")
                .HasFilter("needs_recalculation = true");
            
            entity.HasIndex(m => new { m.DependsOn, m.RangeStart, m.RangeEnd },
                "metrics_range_depends_index");
            
            entity.HasIndex(m => new { m.Discriminator, m.RangeStart, m.RangeEnd, m.DimensionHash }, 
                    "metrics_range_start_end_discriminator_u_index")
                .IsUnique();
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.NeedsRecalculation)
                .HasColumnName("needs_recalculation")
                .HasDefaultValue(false);
            entity.Property(m => m.DependsOn)
                .HasConversion<long>()
                .HasColumnName("depends_on");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.RecalculatedAt).HasColumnName("recalculated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.RangeStart).HasColumnName("range_start");
            entity.Property(e => e.RangeEnd).HasColumnName("range_end");
            entity.Property(e => e.Discriminator).HasColumnName("discriminator");
            entity.Property(e => e.DimensionKey)
                .HasColumnName("dimension_key")
                .HasMaxLength(200);
            entity.Property(e => e.DimensionHash).HasColumnName("dimension_hash")
                .HasColumnType("bytea");
            
            entity.Property(e => e.Json).HasColumnName("json");
            
            entity.HasDiscriminator(e => e.Discriminator);

            entity.HasOne(d => d.Currency).WithMany(p => p.Metrics)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("metrics_currencies_id_fk");
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
            entity.Property(e => e.TotalSum).HasColumnName("total_sum");

            entity.HasOne(d => d.Currency).WithMany(p => p.PurchasesFacts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchases_fact_currencies_id_fk");
        });

        modelBuilder.Entity<SaleContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_contents_pk");

            entity.ToTable("sale_contents");

            entity.HasIndex(e => e.ArticleId, "sale_contents_article_id_index");

            entity.HasIndex(e => e.SaleId, "sale_contents_sale_id_index");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.SaleId)
                .HasMaxLength(128)
                .HasColumnName("sale_id");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleContents)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("sale_contents_sales_fact_id_fk");
        });

        modelBuilder.Entity<SaleContentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_content_detail_pk");

            entity.ToTable("sale_content_detail");

            entity.HasIndex(e => e.CurrencyId, "sale_content_detail_currency_id_index");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BuyPrice).HasColumnName("buy_price");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");

            entity.HasOne(d => d.Currency).WithMany(p => p.SaleContentDetails)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_content_detail_currencies_id_fk");
        });

        modelBuilder.Entity<SalesFact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sales_fact_pk");

            entity.ToTable("sales_fact");

            entity.HasIndex(e => e.BuyerId, "sales_fact_buyer_id_index");

            entity.HasIndex(e => e.CreatedAt, "sales_fact_created_at_index");

            entity.HasIndex(e => e.CurrencyId, "sales_fact_currency_id_index");

            entity.Property(e => e.Id)
                .HasMaxLength(128)
                .HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.TotalSum).HasColumnName("total_sum");

            entity.HasOne(d => d.Currency).WithMany(p => p.SalesFacts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_fact_currencies_id_fk");
        });

        modelBuilder.AllDateTimesToUtc();
            
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

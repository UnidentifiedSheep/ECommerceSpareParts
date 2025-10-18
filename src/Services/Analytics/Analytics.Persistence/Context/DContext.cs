﻿using Analytics.Core.Entities;
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

    public virtual DbSet<SellInfo> SellInfos { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currencies_pk");

            entity.ToTable("currencies");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ToUsd).HasColumnName("to_usd");
        });

        modelBuilder.Entity<SellInfo>(entity =>
        {
            entity.ToTable("sell_info");
            
            entity.HasKey(e => e.SellContentId)
                .HasName("sell_info_pk");

            entity.HasIndex(e => e.ArticleId, "sell_info_article_id_index");

            entity.HasIndex(e => new { e.ArticleId, e.StorageName }, "sell_info_article_id_storage_name_index");

            entity.HasIndex(e => e.BuyCurrencyId, "sell_info_buy_currency_id_index");

            entity.HasIndex(e => e.BuyPrices, "sell_info_buy_prices_index");

            entity.HasIndex(e => e.Markup, "sell_info_markup_index");

            entity.HasIndex(e => e.SellCurrencyId, "sell_info_sell_currency_id_index");

            entity.HasIndex(e => e.SellPrice, "sell_info_sell_price_index");

            entity.HasIndex(e => e.StorageName, "sell_info_storage_name_index");

            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.BuyCurrencyId).HasColumnName("buy_currency_id");
            entity.Property(e => e.BuyPrices).HasColumnName("buy_prices");
            entity.Property(e => e.Markup).HasColumnName("markup");
            entity.Property(e => e.SellContentId).HasColumnName("sell_content_id");
            entity.Property(e => e.SellCurrencyId).HasColumnName("sell_currency_id");
            entity.Property(e => e.SellPrice).HasColumnName("sell_price");
            entity.Property(e => e.StorageName).HasColumnName("storage_name");

            entity.HasOne(d => d.BuyCurrency).WithMany()
                .HasForeignKey(d => d.BuyCurrencyId)
                .HasConstraintName("sell_info_currencies_id_fk");

            entity.HasOne(d => d.SellCurrency).WithMany()
                .HasForeignKey(d => d.SellCurrencyId)
                .HasConstraintName("sell_info_currencies_id_fk_2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

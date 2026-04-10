using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id).HasName("articles_id_pk");

            builder.ToTable("articles");

            builder.HasIndex(e => e.Sku, "articles_sku_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.HasIndex(e => e.CategoryId, "articles_category_id_index");

            builder.HasIndex(e => new { e.NormalizedSku, e.ProducerId },
                "articles_normalized_sku_producer_id_index")
                .IsUnique();

            builder.HasIndex(e => e.Popularity, "articles_popularity_index");

            builder.HasIndex(e => e.ProducerId, "articles_producer_id_index");

            builder.HasIndex(e => e.Stock, "articles_total_count_index");

            builder.HasIndex(e => e.NormalizedSku, "normalized_sku_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.Property(e => e.Id)
                .HasColumnName("id");
            
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("article_name");
            
            builder.Property(e => e.Sku)
                .HasMaxLength(128)
                .HasColumnName("sku");
            
            builder.Property(e => e.CategoryId)
                .HasColumnName("category_id");
            
            builder.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            
            builder.Property(e => e.Indicator)
                .HasMaxLength(24)
                .HasColumnName("indicator");
        
            
            builder.Property(e => e.NormalizedSku)
                .HasMaxLength(128)
                .HasColumnName("normalized_sku");
            
            builder.Property(e => e.PackingUnit)
                .HasColumnName("packing_unit");
            
            builder.Property(e => e.Popularity)
                .HasDefaultValue(1L)
                .HasColumnName("popularity");
            
            builder.Property(e => e.ProducerId)
                .HasColumnName("producer_id");
            
            builder.Property(e => e.Stock)
                .HasColumnName("stock");

            builder.HasOne(d => d.Category).WithMany(p => p.Articles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("articles_categories_id_fk");

            builder.HasOne(d => d.Producer).WithMany(p => p.Articles)
                .HasForeignKey(d => d.ProducerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("producer_id_fk");

            builder.HasMany(d => d.ArticleCrosses).WithMany(p => p.Articles)
                .UsingEntity<Dictionary<string, object>>(
                    "ArticleCross",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("ArticleCrossId")
                        .HasConstraintName("article_crosses_articles_id_fk_2"),
                    l => l.HasOne<Product>().WithMany()
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

            builder.HasMany(d => d.Articles).WithMany(p => p.ArticleCrosses)
                .UsingEntity<Dictionary<string, object>>(
                    "ArticleCross",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("ArticleId")
                        .HasConstraintName("article_crosses_articles_id_fk"),
                    l => l.HasOne<Product>().WithMany()
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
    }
}
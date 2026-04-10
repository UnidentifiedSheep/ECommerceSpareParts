using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductConfiguration : IEntityTypeConfiguration<Entities.Product>
{
    public void Configure(EntityTypeBuilder<Entities.Product> builder)
    {
        builder.HasKey(e => e.Id).HasName("products_id_pk");

            builder.ToTable("products");

            builder.HasIndex(e => e.Sku, "products_sku_index")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.HasIndex(e => e.CategoryId, "products_category_id_index");
            
            builder.HasIndex(e => e.PairId, "products_pair_id_index")
                .IsUnique();

            builder.HasIndex(e => new { e.NormalizedSku, e.ProducerId },
                "products_normalized_sku_producer_id_index")
                .IsUnique();

            builder.HasIndex(e => e.Popularity, "products_popularity_index");

            builder.HasIndex(e => e.ProducerId, "products_producer_id_index");

            builder.HasIndex(e => e.Stock, "products_total_count_index");

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
                .HasConstraintName("products_categories_id_fk");

            builder.HasOne(d => d.Producer).WithMany(p => p.Articles)
                .HasForeignKey(d => d.ProducerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("producer_id_fk");

            builder.HasOne(p => p.Pair)
                .WithOne()
                .HasForeignKey<Entities.Product>(p => p.PairId)
                .OnDelete(DeleteBehavior.SetNull);
    }
}
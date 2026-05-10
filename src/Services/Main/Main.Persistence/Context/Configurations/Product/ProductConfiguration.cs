using EFCore.ComplexIndexes;
using EFCore.ComplexIndexes.PostgreSQL;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductConfiguration : IEntityTypeConfiguration<Entities.Product.Product>
{
    public void Configure(EntityTypeBuilder<Entities.Product.Product> builder)
    {
        builder.HasKey(e => e.Id).HasName("products_id_pk");

        builder.ToTable("products", "public");

        builder.HasIndex(e => e.CategoryId, "products_category_id_index");

        builder.HasIndex(e => e.PairId, "products_pair_id_index")
            .IsUnique();

        builder.HasIndex(e => e.Popularity, "products_popularity_index");

        builder.HasIndex(e => e.ProducerId, "products_producer_id_index");

        builder.HasComplexCompositeIndex(
            x => new { x.Sku.NormalizedValue, x.ProducerId },
            indexName: "products_normalized_sku_producer_id_index",
            isUnique: true);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.ComplexProperty(x => x.Sku,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("sku");

                b.Property(e => e.NormalizedValue)
                    .HasColumnName("normalized_sku")
                    .HasComplexIndex(e =>
                    {
                        e.UseGin().HasName("products_sku_index")
                            .HasOperators("gin_trgm_ops");
                    });
            });

        builder.OwnsOne(
            e => e.Name,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("name")
                    .HasMaxLength(255);
            });

        builder.ComplexProperty(
            e => e.Stock,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("stock")
                    .HasComplexIndex(indexName: "products_stock_index");
            });

        builder.Property(e => e.Indicator)
            .HasConversion(e => e == null ? null : e.Value, x => new Indicator(x))
            .HasMaxLength(24)
            .HasColumnName("indicator");

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id");

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("description");

        builder.Property(e => e.PackingUnit)
            .HasColumnName("packing_unit");

        builder.Property(e => e.Popularity)
            .HasDefaultValue(1L)
            .HasColumnName("popularity");

        builder.Property(e => e.ProducerId)
            .HasColumnName("producer_id");

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();


        builder.HasOne(d => d.Category)
            .WithMany(p => p.Articles)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("products_categories_id_fk");

        builder.HasOne(d => d.Producer)
            .WithMany()
            .HasForeignKey(d => d.ProducerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("producer_id_fk");

        builder.HasOne(p => p.Pair)
            .WithOne()
            .HasForeignKey<Entities.Product.Product>(p => p.PairId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(e => e.Characteristics)
            .HasField("_characteristics")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.Eans)
            .HasField("_eans")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.Images)
            .HasField("_images")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
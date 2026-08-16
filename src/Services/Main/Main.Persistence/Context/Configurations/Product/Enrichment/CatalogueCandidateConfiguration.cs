using EFCore.ComplexIndexes;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProducerEntity = Main.Entities.Producer.Producer;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Persistence.Context.Configurations.Product.Enrichment;

public class CatalogueCandidateConfiguration :
    IEntityTypeConfiguration<CatalogueCandidate>
{
    public void Configure(
        EntityTypeBuilder<CatalogueCandidate> builder)
    {
        builder.ToTable(
            "catalogue_candidates",
            "catalogue_enrichment");

        builder.HasKey(x => x.Id)
            .HasName("catalogue_candidates_pk");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.ComplexProperty(
            x => x.Sku,
            sku =>
            {
                sku.Property(x => x.Value)
                    .HasColumnName("sku")
                    .HasMaxLength(128)
                    .IsRequired();

                sku.Property(x => x.NormalizedValue)
                    .HasColumnName("normalized_sku")
                    .HasMaxLength(128)
                    .IsRequired();
            });

        builder.Property(x => x.ProducerId)
            .HasColumnName("producer_id")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id");

        builder.HasOne<ProducerEntity>(e => e.Producer)
            .WithMany()
            .HasForeignKey(x => x.ProducerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(
                "catalogue_candidates_producer_id_fk");

        builder.HasOne<ProductEntity>(e => e.Product)
            .WithOne()
            .HasForeignKey<CatalogueCandidate>(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName(
                "catalogue_candidates_product_id_fk");

        builder.HasComplexCompositeIndex(
            x => new
            {
                x.Sku.NormalizedValue,
                x.ProducerId
            },
            indexName: "catalogue_candidates_sku_producer_uidx",
            isUnique: true);

        builder.HasIndex(x => x.ProductId)
            .HasDatabaseName(
                "catalogue_candidates_product_id_uidx")
            .IsUnique()
            .HasFilter("product_id IS NOT NULL");

        builder.Navigation(x => x.SupplierProducts)
            .HasField("_supplierProducts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

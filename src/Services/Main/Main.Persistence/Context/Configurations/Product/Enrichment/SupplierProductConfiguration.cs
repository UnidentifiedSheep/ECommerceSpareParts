using EFCore.ComplexIndexes;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product.Enrichment;

public class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
{
	public void Configure(EntityTypeBuilder<SupplierProduct> builder)
	{
		builder.ToTable("supplier_products", "catalogue_enrichment");

		builder.HasKey(x => x.Id).HasName("supplier_products_pk");

		builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

		builder.ComplexProperty(
			x => x.Sku,
			sku =>
			{
				sku.Property(x => x.Value).HasColumnName("sku").HasMaxLength(128).IsRequired();

				sku
					.Property(x => x.NormalizedValue)
					.HasColumnName("normalized_sku")
					.HasMaxLength(128)
					.IsRequired()
					.HasComplexIndex(indexName: "supplier_products_normalized_sku_idx");
			});

		builder.Property(x => x.Producer).HasColumnName("producer").HasMaxLength(256).IsRequired();

		builder.Property(x => x.Supplier).HasColumnName("supplier").HasMaxLength(32).IsRequired();

		builder.Property(x => x.CatalogueCandidateId).HasColumnName("catalogue_candidate_id");

		builder
			.HasOne(x => x.CatalogueCandidate)
			.WithMany(x => x.SupplierProducts)
			.HasForeignKey(x => x.CatalogueCandidateId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("supplier_products_catalogue_candidate_id_fk");

		builder
			.HasMany(x => x.Names)
			.WithOne(x => x.SupplierProduct)
			.HasForeignKey(x => x.SupplierProductId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation(x => x.Names).HasField("_names").UsePropertyAccessMode(PropertyAccessMode.Field);

		builder.HasComplexCompositeIndex(
			x => new
			{
				x.Supplier,
				x.Producer,
				x.Sku.NormalizedValue
			},
			indexName: "supplier_products_supplier_producer_sku_uidx",
			isUnique: true);

		builder
			.HasIndex(x => x.CatalogueCandidateId)
			.HasDatabaseName("supplier_products_catalogue_candidate_id_idx");
	}
}

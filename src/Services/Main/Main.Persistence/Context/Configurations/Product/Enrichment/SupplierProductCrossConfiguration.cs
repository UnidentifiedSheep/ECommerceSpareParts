using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product.Enrichment;

public class SupplierProductCrossConfiguration : IEntityTypeConfiguration<SupplierProductCross>
{
	public void Configure(EntityTypeBuilder<SupplierProductCross> builder)
	{
		builder.ToTable("supplier_product_crosses", "catalogue_enrichment");

		builder
			.HasKey(x => new
			{
				x.LeftId, x.RightId
			})
			.HasName("supplier_product_crosses_pk");

		builder.Property(x => x.LeftId).HasColumnName("left_supplier_product_id").IsRequired();

		builder.Property(x => x.RightId).HasColumnName("right_supplier_product_id").IsRequired();

		builder
			.HasOne<SupplierProduct>()
			.WithMany()
			.HasForeignKey(x => x.LeftId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("supplier_product_crosses_left_product_id_fk");

		builder
			.HasOne<SupplierProduct>()
			.WithMany()
			.HasForeignKey(x => x.RightId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("supplier_product_crosses_right_product_id_fk");

		builder.HasIndex(x => x.RightId).HasDatabaseName("supplier_product_crosses_right_product_id_idx");
	}
}

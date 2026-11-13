using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product.Enrichment;

public class SupplierProductNameConfiguration : IEntityTypeConfiguration<SupplierProductName>
{
	public void Configure(EntityTypeBuilder<SupplierProductName> builder)
	{
		builder.ToTable("supplier_product_names", "catalogue_enrichment");

		builder.HasKey(x => x.Id).HasName("supplier_product_names_pk");

		builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

		builder.Property(x => x.SupplierProductId).HasColumnName("supplier_product_id").IsRequired();

		builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(512).IsRequired();

		builder
			.HasIndex(x => new
			{
				x.SupplierProductId, x.Name
			})
			.HasDatabaseName("supplier_product_names_product_name_uidx")
			.IsUnique();

		builder
			.HasOne(x => x.SupplierProduct)
			.WithMany(x => x.Names)
			.HasForeignKey(x => x.SupplierProductId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("supplier_product_names_supplier_product_id_fk");
	}
}

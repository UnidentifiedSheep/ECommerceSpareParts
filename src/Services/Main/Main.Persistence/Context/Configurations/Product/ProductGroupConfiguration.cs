using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductGroupConfiguration : IEntityTypeConfiguration<ProductGroup>
{
	public void Configure(EntityTypeBuilder<ProductGroup> builder)
	{
		builder.ToTable("product_groups", "public");

		builder.HasKey(e => e.Id).HasName("product_groups_pk");

		builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

		builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(256).IsRequired();

		builder.Property(e => e.NormalizedName)
			.HasColumnName("normalized_name")
			.HasMaxLength(256)
			.IsRequired();

		builder.HasIndex(e => e.NormalizedName, "product_groups_normalized_name_uindex")
			.IsUnique();
	}
}

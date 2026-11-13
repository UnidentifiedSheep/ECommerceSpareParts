using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductCharacteristicConfiguration : IEntityTypeConfiguration<ProductCharacteristic>
{
	public void Configure(EntityTypeBuilder<ProductCharacteristic> builder)
	{
		builder.ToTable("product_characteristics", "public");
		builder.HasKey(e => new { e.ProductId, e.CharacteristicId }).HasName("product_characteristics_pk");
		builder.HasIndex(e => e.ProductId).HasDatabaseName("product_characteristics_id_index");
		builder.HasIndex(e => new { e.CharacteristicId, e.Value })
			.HasDatabaseName("product_characteristics_characteristic_id_value_index");
		builder.Property(e => e.ProductId).HasColumnName("product_id");
		builder.Property(e => e.CharacteristicId).HasColumnName("characteristic_id");
		builder.Property(e => e.Value).HasMaxLength(128).HasColumnName("value");
		builder.HasOne<Entities.Product.Product>()
			.WithMany(p => p.Characteristics)
			.HasForeignKey(x => x.ProductId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("product_characteristics_product_id_fk");
		builder.HasOne(e => e.Characteristic)
			.WithMany()
			.HasForeignKey(e => e.CharacteristicId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("product_characteristics_characteristic_id_fk");
	}
}

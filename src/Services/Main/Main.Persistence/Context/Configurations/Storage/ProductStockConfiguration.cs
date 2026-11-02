using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public sealed class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
	public void Configure(EntityTypeBuilder<ProductStock> builder)
	{
		builder.ToTable("product_stocks", "public", table =>
		{
			table.HasCheckConstraint("product_stocks_quantity_nonnegative_ck", "quantity >= 0");
		});

		builder.HasKey(stock => new { stock.StorageCode, stock.ProductId }).HasName("product_stocks_pk");
		builder.Property(stock => stock.StorageCode).HasColumnName("storage_code").HasMaxLength(128);
		builder.Property(stock => stock.ProductId).HasColumnName("product_id").ValueGeneratedNever();
		builder.Property(stock => stock.Quantity).HasColumnName("quantity");

		builder.HasIndex(stock => stock.ProductId, "product_stocks_product_id_idx");

		builder.HasOne(stock => stock.Storage)
			.WithMany()
			.HasForeignKey(stock => stock.StorageCode)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("product_stocks_storage_code_fk");

		builder.HasOne(stock => stock.Product)
			.WithMany()
			.HasForeignKey(stock => stock.ProductId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("product_stocks_product_id_fk");
	}
}

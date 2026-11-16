using Main.Entities.Product.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Product.Persistence.Configurations;

public sealed class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
	public void Configure(EntityTypeBuilder<ProductStock> builder)
	{
		builder.ToTable(
			"product_stocks",
			"public",
			table =>
			{
				table.HasCheckConstraint("product_stocks_quantity_nonnegative_ck", "quantity >= 0");
			});

		builder.HasKey(stock => new { stock.StorageCode, stock.ProductId }).HasName("product_stocks_pk");
		builder.Property(stock => stock.StorageCode).HasColumnName("storage_code").HasMaxLength(128);
		builder.Property(stock => stock.ProductId).HasColumnName("product_id").ValueGeneratedNever();
		builder.Property(stock => stock.Quantity).HasColumnName("quantity");

		builder.HasIndex(stock => stock.ProductId, "product_stocks_product_id_idx");

		builder.HasOne(stock => stock.Product)
			.WithMany(e => e.Stocks)
			.HasForeignKey(stock => stock.ProductId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("product_stocks_product_id_fk");
	}
}

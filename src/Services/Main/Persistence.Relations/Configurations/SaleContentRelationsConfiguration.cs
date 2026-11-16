using Main.Entities.Product.Product;
using Main.Entities.Sale.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class SaleContentRelationsConfiguration : IEntityTypeConfiguration<SaleContent>
{
	public void Configure(EntityTypeBuilder<SaleContent> builder)
		=> builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_content_products_id_fk");
}

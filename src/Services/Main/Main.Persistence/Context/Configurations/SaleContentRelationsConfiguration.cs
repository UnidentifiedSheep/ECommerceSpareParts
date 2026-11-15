using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class SaleContentRelationsConfiguration : IEntityTypeConfiguration<Entities.Sale.SaleContent>
{
	public void Configure(EntityTypeBuilder<Entities.Sale.SaleContent> builder)
	{
		builder
			.HasOne<Entities.Product.Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("sale_content_products_id_fk");
	}
}

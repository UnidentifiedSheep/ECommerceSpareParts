using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class CartRelationsConfiguration : IEntityTypeConfiguration<Entities.Cart.Cart>
{
	public void Configure(EntityTypeBuilder<Entities.Cart.Cart> builder)
	{
		builder
			.HasOne<Entities.Product.Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.HasConstraintName("cart_product_id_fk");

		builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(d => d.UserId)
			.HasConstraintName("cart_users_id_fk");
	}
}

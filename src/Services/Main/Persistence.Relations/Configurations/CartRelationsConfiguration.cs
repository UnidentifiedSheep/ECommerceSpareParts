using Main.Entities.Cart.Cart;
using Main.Entities.Product.Product;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class CartRelationsConfiguration : IEntityTypeConfiguration<Cart>
{
	public void Configure(EntityTypeBuilder<Cart> builder)
	{
		builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.HasConstraintName("cart_product_id_fk");

		builder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(d => d.UserId)
			.HasConstraintName("cart_users_id_fk");
	}
}

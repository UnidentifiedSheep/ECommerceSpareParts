using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Cart.Persistence.Configurations.Cart;

public class CartConfiguration : IEntityTypeConfiguration<Entities.Cart.Cart.Cart>
{
	public void Configure(EntityTypeBuilder<Entities.Cart.Cart.Cart> builder)
	{
		builder.ToTable("cart", "public");

		builder
			.HasKey(e => new
			{
				e.UserId, e.ProductId
			})
			.HasName("cart_pk");

		builder.HasIndex(e => e.ProductId).HasDatabaseName("cart_product_id_idx");

		builder.Property(e => e.UserId).HasColumnName("user_id");

		builder.Property(e => e.ProductId).HasColumnName("product_id");

		builder.Property(e => e.Count).HasColumnName("count");
	}
}

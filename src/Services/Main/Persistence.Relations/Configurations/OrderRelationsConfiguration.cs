using Main.Entities.Order.Order;
using Main.Entities.Organization.Organization;
using Main.Entities.Product.Product;
using Main.Entities.Sale.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class OrderRelationsConfiguration : IEntityTypeConfiguration<Order>
{
	public void Configure(EntityTypeBuilder<Order> builder)
	{
		builder
			.HasOne<Organization>()
			.WithMany()
			.HasForeignKey(e => e.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("orders_organization_id_fk");

		builder
			.HasOne<Main.Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("orders_currency_id_fk");

		builder
			.HasOne<Main.Entities.User.User>()
			.WithMany()
			.HasForeignKey(d => d.UserId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("orders_users_id_fk");

		builder
			.HasOne<Main.Entities.User.User>()
			.WithMany()
			.HasForeignKey(e => e.ConfirmedByUserId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("orders_confirmed_by_user_id_fk");
	}
}

public sealed class OrderItemRelationsConfiguration : IEntityTypeConfiguration<OrderItem>
{
	public void Configure(EntityTypeBuilder<OrderItem> builder)
		=> builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("order_items_products_id_fk");
}

public sealed class OrderFulfillmentRelationsConfiguration : IEntityTypeConfiguration<OrderFulfillment>
{
	public void Configure(EntityTypeBuilder<OrderFulfillment> builder)
		=> builder
			.HasOne<SaleContent>()
			.WithMany()
			.HasForeignKey(item => item.SaleContentId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("order_fulfillments_sale_content_id_fk");
}

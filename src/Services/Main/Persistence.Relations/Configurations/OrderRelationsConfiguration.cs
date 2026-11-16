using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class OrderRelationsConfiguration : IEntityTypeConfiguration<Entities.Order.Order>
{
	public void Configure(EntityTypeBuilder<Entities.Order.Order> builder)
	{
		builder
			.HasOne<Entities.Organization.Organization>()
			.WithMany()
			.HasForeignKey(e => e.OrganizationId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("orders_organization_id_fk");

		builder
			.HasOne<Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("orders_currency_id_fk");

		builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(d => d.UserId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("orders_users_id_fk");

		builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(e => e.ConfirmedByUserId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("orders_confirmed_by_user_id_fk");
	}
}

public sealed class OrderItemRelationsConfiguration : IEntityTypeConfiguration<Entities.Order.OrderItem>
{
	public void Configure(EntityTypeBuilder<Entities.Order.OrderItem> builder)
		=> builder
			.HasOne<Entities.Product.Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("order_items_products_id_fk");
}

public sealed class OrderFulfillmentRelationsConfiguration : IEntityTypeConfiguration<Entities.Order.OrderFulfillment>
{
	public void Configure(EntityTypeBuilder<Entities.Order.OrderFulfillment> builder)
		=> builder
			.HasOne<Entities.Sale.SaleContent>()
			.WithMany()
			.HasForeignKey(item => item.SaleContentId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("order_fulfillments_sale_content_id_fk");
}

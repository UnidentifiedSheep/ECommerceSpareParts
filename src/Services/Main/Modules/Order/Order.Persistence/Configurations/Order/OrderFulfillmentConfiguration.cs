using Main.Entities.Order.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Order.Persistence.Configurations.Order;

public sealed class OrderFulfillmentConfiguration : IEntityTypeConfiguration<OrderFulfillment>
{
	public void Configure(EntityTypeBuilder<OrderFulfillment> builder)
	{
		builder.ToTable("order_fulfillments", "public");
		builder.HasKey(item => new { item.OrderItemId, item.SaleContentId }).HasName("order_fulfillments_pk");
		builder.Property(item => item.OrderItemId).HasColumnName("order_item_id");
		builder.Property(item => item.SaleContentId).HasColumnName("sale_content_id");
		builder.Property(item => item.CoveredQuantity).HasColumnName("covered_quantity");
		builder.HasIndex(item => item.SaleContentId, "order_fulfillments_sale_content_id_index");

		builder.HasOne(item => item.OrderItem)
			.WithMany()
			.HasForeignKey(item => item.OrderItemId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("order_fulfillments_order_item_id_fk");
	}
}

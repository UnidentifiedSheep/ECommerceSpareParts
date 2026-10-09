using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Order;

public class OrderConfiguration : IEntityTypeConfiguration<Entities.Order.Order>
{
	public void Configure(EntityTypeBuilder<Entities.Order.Order> builder)
	{
		builder.ToTable("orders", "public");

		builder.HasKey(e => e.Id).HasName("orders_pk");

		builder.HasIndex(e => e.CurrencyId, "orders_currency_id_index");

		builder.HasIndex(e => e.OrganizationId, "orders_organization_id_index");

		builder.HasIndex(e => e.Status, "orders_status_index");

		builder.HasIndex(e => e.FulfillmentStatus, "orders_fulfillment_status_index");

		builder.HasIndex(e => e.UserId, "orders_user_id_index");

		builder.HasIndex(e => e.ConfirmedByUserId, "orders_confirmed_by_user_id_index");

		builder
			.Property(e => e.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();

		builder.Property(e => e.CurrencyId).HasColumnName("currency_id");

		builder.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");

		builder.Property(e => e.ConfirmedByUserId).HasColumnName("confirmed_by_user_id");

		builder.Property(e => e.FulfillmentStatus).HasColumnName("fulfillment_status");

		builder.Property(e => e.OrganizationId).HasColumnName("organization_id");

		builder.Property(e => e.Source).HasColumnName("source");

		builder.Property(e => e.Status).HasColumnName("status");

		builder.Property(e => e.UserId).HasColumnName("user_id");

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

		builder
			.Navigation(e => e.Items)
			.HasField("_items")
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}

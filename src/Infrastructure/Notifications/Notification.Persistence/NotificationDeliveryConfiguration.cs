using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Persistence;

public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
	public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
	{
		builder.ToTable("notification_deliveries", "notification");

		builder
			.HasKey(e => new
			{
				e.NotificationId,
				e.ChannelSystemName
			})
			.HasName("notification_deliveries_pk");

		builder.Property(e => e.NotificationId).HasColumnName("notification_id");

		builder
			.Property(e => e.ChannelSystemName)
			.HasColumnName("channel_system_name")
			.HasMaxLength(128)
			.ValueGeneratedNever();

		builder.Property(e => e.Status).HasColumnName("status").HasConversion<string>();

		builder.Property(e => e.Attempts).HasColumnName("attempts");

		builder.Property(e => e.Error).HasColumnName("error");

		builder.Property(e => e.DeliveredAt).HasColumnName("delivered_at");

		builder.HasIndex(
			e => new
			{
				e.ChannelSystemName,
				e.Status,
				e.NotificationId
			},
			"notification_deliveries_channel_status_notification_id_idx");
	}
}

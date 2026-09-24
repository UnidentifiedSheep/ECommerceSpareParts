using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationEntity = Notification.Core.Entities.Notification;

namespace Notification.Persistence;

public class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
	public void Configure(EntityTypeBuilder<NotificationEntity> builder)
	{
		builder.ToTable("notifications", "notification");

		builder.HasKey(e => e.Id).HasName("notifications_pk");

		builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

		builder.Property(e => e.UserId).HasColumnName("user_id");

		builder
			.Property(e => e.NotificationSystemName)
			.HasColumnName("notification_system_name")
			.HasMaxLength(128);

		builder.Property(e => e.Model).HasColumnName("model").HasColumnType("jsonb");

		builder
			.Property(e => e.Culture)
			.HasColumnName("culture")
			.HasMaxLength(128)
			.HasConversion(
				culture => culture == null ? null : culture.Name,
				name => name == null ? null : CultureInfo.GetCultureInfo(name));

		builder.Property(e => e.CreateAt).HasColumnName("created_at");

		builder.HasIndex(
			e => new
			{
				e.UserId,
				e.CreateAt
			},
			"notifications_user_id_created_at_idx");

		builder
			.HasMany(e => e.Deliveries)
			.WithOne(e => e.Notification)
			.HasForeignKey(e => e.NotificationId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("notification_deliveries_notification_id_fk");

		builder
			.Navigation(e => e.Deliveries)
			.HasField("_deliveries")
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}

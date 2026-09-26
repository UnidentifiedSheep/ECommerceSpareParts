using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Persistence;

public sealed class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
	public void Configure(EntityTypeBuilder<InAppNotification> builder)
	{
		builder.ToTable("in_app_notifications", "notification");

		builder.HasKey(e => e.Id).HasName("in_app_notifications_pk");

		builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
		builder.Property(e => e.UserId).HasColumnName("user_id");
		builder.Property(e => e.Text).HasColumnName("text").HasColumnType("text");
		builder.Property(e => e.CreateAt).HasColumnName("created_at");
		builder.Property(e => e.SeenAt).HasColumnName("seen_at");

		builder.HasIndex(
			e => new
			{
				e.UserId,
				e.CreateAt
			},
			"in_app_notifications_user_id_created_at_idx");
	}
}

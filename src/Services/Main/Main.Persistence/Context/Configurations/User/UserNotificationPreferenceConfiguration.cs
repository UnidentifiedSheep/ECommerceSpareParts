using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
	public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
	{
		builder.ToTable("user_notification_preferences", "auth");

		builder
			.HasKey(e => new { e.UserId, e.ChannelName })
			.HasName("user_notification_preferences_pk");

		builder.Property(e => e.UserId).HasColumnName("user_id");
		builder.Property(e => e.ChannelName).HasMaxLength(128).HasColumnName("channel_name");
		builder.Property(e => e.Enabled).HasColumnName("enabled");

		builder
			.HasOne<Entities.User.User>()
			.WithMany(user => user.NotificationPreferences)
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("user_notification_preferences_users_id_fk");
	}
}

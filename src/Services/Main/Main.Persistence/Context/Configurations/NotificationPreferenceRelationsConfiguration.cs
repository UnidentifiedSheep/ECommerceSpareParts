using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class NotificationPreferenceRelationsConfiguration
	: IEntityTypeConfiguration<Entities.User.UserNotificationPreference>
{
	public void Configure(EntityTypeBuilder<Entities.User.UserNotificationPreference> builder)
		=> builder
			.HasOne<Entities.User.User>()
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("user_notification_preferences_users_id_fk");
}

using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class NotificationPreferenceRelationsConfiguration
	: IEntityTypeConfiguration<UserNotificationPreference>
{
	public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
		=> builder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("user_notification_preferences_users_id_fk");
}

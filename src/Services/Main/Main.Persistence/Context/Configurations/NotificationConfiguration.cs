using Main.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
	public void Configure(EntityTypeBuilder<Notification> builder)
	{
		builder.ToTable("notifications", "public");

		builder.HasKey(e => e.Id).HasName("notification_pk");

		builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
		builder.Property(e => e.Message).HasColumnName("message").IsRequired();
		builder.Property(e => e.Title).HasColumnName("title").IsRequired();

		builder.Property(e => e.PublishedAt).HasColumnName("publishedAt").IsRequired();
		builder.Property(e => e.SeenAt).HasColumnName("seenAt");

		builder.Property(e => e.UserId).HasColumnName("user_id");

		builder.HasIndex(e => new { e.UserId, e.PublishedAt })
			.HasDatabaseName("notification_user_published_at_idx");

		builder
			.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.HasConstraintName("notification_user_fk");
	}
}

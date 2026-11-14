using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Auth;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
	public void Configure(EntityTypeBuilder<UserToken> builder)
	{
		builder.ToTable("user_tokens", "auth");

		builder.HasKey(e => e.Id).HasName("user_tokens_pk");

		builder
			.HasIndex(e => e.ExpiresAt)
			.HasFilter("((revoked = false) AND (expires_at IS NOT NULL))")
			.HasDatabaseName("user_tokens_expires_at_index");

		builder.HasIndex(e => e.TokenHash).HasDatabaseName("user_tokens_token_hash_uindex").IsUnique();

		builder.HasIndex(e => e.UserId).HasDatabaseName("user_tokens_user_id_index");

		builder
			.Property(e => e.Id)
			.HasColumnName("id");

		builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");

		builder.Property(e => e.RevokeReason).HasMaxLength(255).HasColumnName("revoke_reason");

		builder.Property(e => e.Revoked).HasColumnName("revoked");

		builder.Property(e => e.TokenHash).HasColumnName("token_hash");

		builder.Property(e => e.Type).HasMaxLength(50).HasColumnName("type");

		builder.Property(e => e.UserId).HasColumnName("user_id");

		builder.Property(e => e.RowVersion).HasColumnName("xmin").IsRowVersion();

		builder
			.HasOne(d => d.User)
			.WithMany()
			.HasForeignKey(d => d.UserId)
			.HasConstraintName("user_tokens_users_id_fk");
	}
}

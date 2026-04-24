using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserConfiguration : IEntityTypeConfiguration<Entities.User.User>
{
    public void Configure(EntityTypeBuilder<Entities.User.User> builder)
    {
        builder.ToTable("users", "auth");
        
        builder.HasKey(e => e.Id)
            .HasName("users_pk");

        builder.HasIndex("normalized_user_name")
            .HasDatabaseName("users_normalized_user_name_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex("normalized_user_name")
            .HasDatabaseName("users_normalized_user_name_uindex")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");
        
        builder.Property(e => e.AccessFailedCount)
            .HasColumnName("access_failed_count");
        
        builder.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
        builder.Property(e => e.LockoutEnd).HasColumnName("lockout_end");

        builder.OwnsOne(
            b => b.UserName,
            b =>
            {
                b.Property(e => e.Value)
                    .HasMaxLength(36)
                    .HasColumnName("user_name");

                b.Property(e => e.NormalizedValue)
                    .HasMaxLength(36)
                    .HasColumnName("normalized_user_name");
            });
        
        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash");
        
        builder.Property(e => e.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled");
    }
}
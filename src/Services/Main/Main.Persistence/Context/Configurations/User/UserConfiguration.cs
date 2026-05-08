using EFCore.ComplexIndexes;
using EFCore.ComplexIndexes.PostgreSQL;
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

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");

        builder.Property(e => e.AccessFailedCount)
            .HasColumnName("access_failed_count");

        builder.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
        builder.Property(e => e.LockoutEnd).HasColumnName("lockout_end");

        builder.ComplexProperty(
            b => b.UserName,
            b =>
            {
                b.Property(e => e.Value)
                    .HasMaxLength(36)
                    .HasColumnName("user_name");

                b.Property(e => e.NormalizedValue)
                    .HasComplexIndex(true, null, "users_normalized_user_name_uindex");

                b.Property(e => e.NormalizedValue)
                    .HasMaxLength(36)
                    .HasColumnName("normalized_user_name")
                    .HasComplexIndex(x =>
                    {
                        x.UseGin()
                            .IsUnique(false)
                            .HasOperators("gin_trgm_ops")
                            .HasName("users_normalized_user_name_index");
                    });
            });

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(e => e.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled");

        builder.Navigation(e => e.Emails)
            .HasField("_emails")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.Permissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.Phones)
            .HasField("_phones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.Roles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.Vehicles)
            .HasField("_vehicles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(e => e.CartItems)
            .HasField("_cartItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
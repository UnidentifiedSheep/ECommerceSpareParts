using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserEmailConfiguration : IEntityTypeConfiguration<UserEmail>
{
    public void Configure(EntityTypeBuilder<UserEmail> builder)
    {
        builder.ToTable("user_emails", "auth");

        builder.HasKey(e => e.Email)
            .HasName("user_emails_primary_key");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("user_emails_user_id_index");

        builder.HasIndex(e => new { e.UserId, e.IsPrimary })
            .HasDatabaseName("user_emails_user_id_is_primary_uindex")
            .IsUnique()
            .HasFilter("(is_primary = true)");

        builder.Property(e => e.Confirmed)
            .HasColumnName("confirmed");

        builder.Property(e => e.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(e => e.EmailType)
            .HasMaxLength(50)
            .HasColumnName("email_type");

        builder.Property(e => e.IsPrimary).HasColumnName("is_primary");

        builder.Property(e => e.Email)
            .HasConversion(e => e.Value, e => new Email(e))
            .HasMaxLength(255)
            .HasColumnName("normalized_email");

        builder.HasIndex(e => e.Email, "user_emails_normalized_email_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne<Entities.User.User>(e => e.User)
            .WithMany(p => p.Emails)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_emails_users_id_fk");
    }
}
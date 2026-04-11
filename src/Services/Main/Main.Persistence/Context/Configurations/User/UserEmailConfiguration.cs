using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserEmailConfiguration : IEntityTypeConfiguration<UserEmail>
{
    public void Configure(EntityTypeBuilder<UserEmail> builder)
    {
        builder.ToTable("user_emails", "auth");
        
        builder.HasKey(e => e.NormalizedEmail)
            .HasName("user_emails_pk");

        builder.HasIndex(e => e.NormalizedEmail)
            .HasDatabaseName("user_emails_normalized_email_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.NormalizedEmail)
            .HasDatabaseName("user_emails_normalized_email_uindex")
            .IsUnique();

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
        
        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");
        
        builder.Property(e => e.EmailType)
            .HasMaxLength(50)
            .HasColumnName("email_type");
        
        builder.Property(e => e.IsPrimary).HasColumnName("is_primary");
        
        builder.Property(e => e.NormalizedEmail)
            .HasMaxLength(255)
            .HasColumnName("normalized_email");
        
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne<Entities.User>()
            .WithMany(p => p.UserEmails)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_emails_users_id_fk");
    }
}
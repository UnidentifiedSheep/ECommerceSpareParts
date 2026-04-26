using Main.Entities;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserEmailConfiguration : IEntityTypeConfiguration<UserEmail>
{
    public void Configure(EntityTypeBuilder<UserEmail> builder)
    {
        builder.ToTable("user_emails", "auth");
        
        builder.HasKey("normalized_email")
            .HasName("user_emails_pk");

        builder.HasIndex("normalized_email")
            .HasDatabaseName("user_emails_normalized_email_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex("normalized_email")
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
        
        builder.Property(e => e.EmailType)
            .HasMaxLength(50)
            .HasColumnName("email_type");
        
        builder.Property(e => e.IsPrimary).HasColumnName("is_primary");

        builder.OwnsOne(
            b => b.Email,
            b =>
            {
                b.Property(e => e.NormalizedValue)
                    .HasMaxLength(255)
                    .HasColumnName("normalized_email");
                
                b.Property(e => e.Value)
                    .HasMaxLength(255)
                    .HasColumnName("email");
            });
        
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne<Entities.User.User>(e => e.User)
            .WithMany(p => p.Emails)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_emails_users_id_fk");
    }
}
using Main.Entities;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserPhoneConfiguration : IEntityTypeConfiguration<UserPhone>
{
    public void Configure(EntityTypeBuilder<UserPhone> builder)
    {
        builder.ToTable("user_phones", "auth");
        
        builder.HasKey(e => e.NormalizedPhone)
            .HasName("user_phones_pk");

        builder.HasIndex(e => e.NormalizedPhone, "user_phones_normalized_phone_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => new { e.UserId, e.IsPrimary }, "user_phones_user_id_is_primary_uindex")
            .IsUnique()
            .HasFilter("(is_primary = true)");

        builder.Property(e => e.Confirmed)
            .HasColumnName("confirmed");
        
        builder.Property(e => e.ConfirmedAt)
            .HasColumnName("confirmed_at");
        
        builder.Property(e => e.IsPrimary)
            .HasColumnName("is_primary");
        
        builder.Property(e => e.NormalizedPhone)
            .HasMaxLength(32)
            .HasColumnName("normalized_phone");
        
        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(32)
            .HasColumnName("phone_number");
        
        builder.Property(e => e.PhoneType)
            .HasMaxLength(32)
            .HasColumnName("phone_type");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne<Entities.User.User>()
            .WithMany(p => p.UserPhones)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_phones_user_id_fkey");
    }
}
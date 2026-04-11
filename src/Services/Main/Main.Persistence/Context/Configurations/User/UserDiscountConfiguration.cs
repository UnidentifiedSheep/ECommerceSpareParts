using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserDiscountConfiguration : IEntityTypeConfiguration<UserDiscount>
{
    public void Configure(EntityTypeBuilder<UserDiscount> builder)
    {
        builder.ToTable("user_discounts");
        
        builder.HasKey(e => e.UserId)
            .HasName("user_discounts_pk");

        builder.Property(e => e.UserId)
            .ValueGeneratedNever()
            .HasColumnName("user_id");
        
        builder.Property(e => e.Discount)
            .HasColumnName("discount");

        builder.HasOne<Entities.User>()
            .WithOne(p => p.UserDiscount)
            .HasForeignKey<UserDiscount>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_discounts_users_id_fk");
    }
}
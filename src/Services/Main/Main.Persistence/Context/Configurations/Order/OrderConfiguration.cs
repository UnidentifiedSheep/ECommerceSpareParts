using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Order;

public class OrderConfiguration : IEntityTypeConfiguration<Entities.Order>
{
    public void Configure(EntityTypeBuilder<Entities.Order> builder)
    {
        builder.ToTable("orders");
        
        builder.HasKey(e => e.Id).HasName("orders_pk");
        
        builder.HasIndex(e => e.BuyerApproved, "orders_buyer_approved_index");

        builder.HasIndex(e => e.CurrencyId, "orders_currency_id_index");

        builder.HasIndex(e => e.IsCanceled, "orders_is_canceled_index");

        builder.HasIndex(e => e.SellerApproved, "orders_seller_approved_index");

        builder.HasIndex(e => e.Status, "orders_status_index");

        builder.HasIndex(e => new { e.UserId, e.IsCanceled }, "orders_user_id_is_canceled_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.BuyerApproved)
            .HasColumnName("buyer_approved");
            
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");

        builder.Property(e => e.IsCanceled)
            .HasColumnName("is_canceled");
        
        builder.Property(e => e.SellerApproved)
            .HasColumnName("seller_approved");
            
        builder.Property(e => e.SignedTotalPrice)
            .HasColumnName("signed_total_price");
        
        builder.Property(e => e.Status)
            .HasColumnName("status");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        
        builder.Property(e => e.WhoUpdated)
            .HasColumnName("who_updated");

        builder.HasOne<Entities.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("orders_currency_id_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("orders_users_id_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.WhoUpdated)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("orders_users_id_fk_2");
    }
}
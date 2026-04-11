using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Order;

public class OrderVersionConfiguration : IEntityTypeConfiguration<OrderVersion>
{
    public void Configure(EntityTypeBuilder<OrderVersion> builder)
    {
        builder.ToTable("order_versions");
        
        builder.HasKey(e => e.Id).HasName("order_versions_pk");

        builder.HasIndex(e => new { e.OrderId, e.Id }, "order_versions_order_id_id_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.BuyerApproved).HasColumnName("buyer_approved");
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.OrderId).HasColumnName("order_id");
        builder.Property(e => e.SellerApproved).HasColumnName("seller_approved");
        builder.Property(e => e.SignedTotalPrice).HasColumnName("signed_total_price");
        builder.Property(e => e.Status).HasColumnName("status");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.WhoUpdated).HasColumnName("who_updated");

        builder.HasOne<Entities.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("order_versions_currency_id_fk");

        builder.HasOne<Entities.Order>()
            .WithMany()
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("order_versions_orders_id_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.WhoUpdated)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("order_versions_users_id_fk");
    }
}
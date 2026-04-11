using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Order;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        
        builder.HasKey(e => e.Id).HasName("order_items_pk");

        builder.HasIndex(e => e.ProductId, "order_items_product_id_index");

        builder.HasIndex(e => e.OrderId, "order_items_order_id_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.ProductId)
            .HasColumnName("article_id");
        
        builder.Property(e => e.Count)
            .HasColumnName("count");
        
        builder.Property(e => e.LockedPrice)
            .HasColumnName("locked_price");
        
        builder.Property(e => e.OrderId)
            .HasColumnName("order_id");
        
        builder.Property(e => e.SignedPrice)
            .HasColumnName("signed_price");

        builder.HasOne<Entities.Product.Product>()
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("order_items_articles_id_fk");

        builder.HasOne<Entities.Order>()
            .WithMany()
            .HasForeignKey(d => d.OrderId)
            .HasConstraintName("order_items_orders_id_fk");
    }
}
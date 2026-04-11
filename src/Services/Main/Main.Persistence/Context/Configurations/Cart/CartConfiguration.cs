using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Cart;

public class CartConfiguration : IEntityTypeConfiguration<Entities.Cart.Cart>
{
    public void Configure(EntityTypeBuilder<Entities.Cart.Cart> builder)
    {
        builder.ToTable("cart");
        
        builder.HasKey(e => new { e.UserId, e.ProductId })
            .HasName("cart_pk");
        
        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("cart_product_id_idx");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        
        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.Count)
            .HasColumnName("count");
        
        builder.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .HasConstraintName("cart_product_id_fk");

        builder.HasOne<Entities.User.User>()
            .WithMany(p => p.CartItems)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("cart_users_id_fk");
    }
}
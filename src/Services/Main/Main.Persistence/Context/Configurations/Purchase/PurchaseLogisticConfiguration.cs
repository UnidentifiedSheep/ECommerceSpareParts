using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Purchase;

public class PurchaseLogisticConfiguration : IEntityTypeConfiguration<PurchaseLogistic>
{
    public void Configure(EntityTypeBuilder<PurchaseLogistic> builder)
    {
        builder.ToTable("purchase_logistics");
        
        builder.HasKey(e => e.PurchaseId).HasName("purchase_logistics_pk");

        builder.HasIndex(e => e.TransactionId, "purchase_logistics_transaction_id_uindex")
            .IsUnique();

        builder.Property(e => e.PurchaseId)
            .HasColumnName("purchase_id");
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.MinimumPrice)
            .HasColumnName("minimum_price");
            
        builder.Property(e => e.MinimumPriceApplied)
            .HasColumnName("minimum_price_applied");
            
        builder.Property(e => e.PriceKg)
            .HasColumnName("price_kg");
            
        builder.Property(e => e.PricePerM3)
            .HasColumnName("price_per_m3");
        
        builder.Property(e => e.PricePerOrder)
            .HasColumnName("price_per_order");
        
        builder.Property(e => e.PricingModel)
            .HasMaxLength(24)
            .HasColumnName("pricing_model");
            
        builder.Property(e => e.RouteId)
            .HasColumnName("route_id");
            
        builder.Property(e => e.RouteType)
            .HasMaxLength(24)
            .HasColumnName("route_type");
            
        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id");

            
        builder.HasOne(d => d.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_logistics_currency_id_fk");

        builder.HasOne<Entities.Purchase>()
            .WithOne(p => p.PurchaseLogistic)
            .HasForeignKey<PurchaseLogistic>(d => d.PurchaseId)
            .HasConstraintName("purchase_logistics_purchase_id_fk");

        builder.HasOne<StorageRoute>()
            .WithMany(p => p.PurchaseLogistics)
            .HasForeignKey(d => d.RouteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_logistics_storage_routes_id_fk");

        builder.HasOne(d => d.Transaction)
            .WithOne()
            .HasForeignKey<PurchaseLogistic>(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_logistics_transactions_id_fk");
    }
}
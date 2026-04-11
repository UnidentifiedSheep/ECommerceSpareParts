using Main.Entities;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class StorageRouteConfiguration : IEntityTypeConfiguration<StorageRoute>
{
    public void Configure(EntityTypeBuilder<StorageRoute> builder)
    {
        builder.ToTable("storage_routes");
        
        builder.HasKey(e => e.Id).HasName("storage_routes_pk");

        builder.HasIndex(e => new { e.FromStorageName, e.ToStorageName, e.IsActive },
                "storage_from_to_active_uindex")
            .IsUnique()
            .HasFilter("(is_active = true)");

        builder.HasIndex(e => e.CarrierId, "storage_routes_carrier_id_index");

        builder.HasIndex(e => e.CurrencyId, "storage_routes_currency_id_index");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.CarrierId)
            .HasColumnName("carrier_id");
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.DeliveryTimeMinutes)
            .HasColumnName("delivery_time_minutes");
        
        builder.Property(e => e.DistanceM)
            .HasColumnName("distance_m");
        
        builder.Property(e => e.FromStorageName)
            .HasMaxLength(128)
            .HasColumnName("from_storage_name");
        
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");
        
        builder.Property(e => e.MinimumPrice)
            .HasColumnName("minimum_price");
        
        builder.Property(e => e.PriceKg)
            .HasColumnName("price_kg");
        
        builder.Property(e => e.PricePerM3)
            .HasColumnName("price_per_m3");
        
        builder.Property(e => e.PricePerOrder)
            .HasColumnName("price_per_order");
        
        builder.Property(e => e.PricingModel)
            .HasMaxLength(24)
            .HasColumnName("pricing_model");
        
        builder.Property(e => e.RouteType)
            .HasMaxLength(24)
            .HasColumnName("route_type");
        
        builder.Property(e => e.ToStorageName)
            .HasMaxLength(128)
            .HasColumnName("to_storage_name");
        
        builder.HasOne<Entities.User.User>()
            .WithMany()
            .HasForeignKey(d => d.CarrierId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("storage_routes_users_id_fk");

        builder.HasOne(d => d.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_routes_currency_id_fk");

        builder.HasOne<Entities.Storage.Storage>()
            .WithMany()
            .HasForeignKey(d => d.FromStorageName)
            .HasConstraintName("storage_routes_storages_name_fk");

        builder.HasOne<Entities.Storage.Storage>()
            .WithMany()
            .HasForeignKey(d => d.ToStorageName)
            .HasConstraintName("storage_routes_storages_name_fk_2");
    }
}
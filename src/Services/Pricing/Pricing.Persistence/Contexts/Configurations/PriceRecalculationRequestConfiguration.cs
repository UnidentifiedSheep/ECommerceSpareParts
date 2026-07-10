using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities;

namespace Pricing.Persistence.Contexts.Configurations;

public class PriceRecalculationRequestConfiguration : IEntityTypeConfiguration<PriceRecalculationRequest>
{
    public void Configure(EntityTypeBuilder<PriceRecalculationRequest> builder)
    {
        builder.ToTable("price_recalculation_requests", "public");

        builder.HasKey(e => new
            {
                e.ProductId,
                e.StorageName
            })
            .HasName("price_recalculation_request_pk");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.StorageName)
            .HasMaxLength(128)
            .HasColumnName("storage_name");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
    }
}

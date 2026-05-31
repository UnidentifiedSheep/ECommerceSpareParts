using Analytics.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Metric;

public class ProductPurchasesMetricConfiguration : IEntityTypeConfiguration<ProductPurchasesMetric>
{
    public void Configure(EntityTypeBuilder<ProductPurchasesMetric> builder)
    {
        builder.HasIndex(e => new { e.Discriminator, e.ProductId },
            "metrics_discriminator_article_index");

        builder.Property(e => e.ProductId).HasColumnName("product_id");
    }
}
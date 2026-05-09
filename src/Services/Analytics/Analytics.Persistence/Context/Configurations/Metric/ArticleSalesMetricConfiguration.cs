using Analytics.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Metric;

public class ArticleSalesMetricConfiguration : IEntityTypeConfiguration<ArticleSalesMetric>
{
    public void Configure(EntityTypeBuilder<ArticleSalesMetric> builder)
    {
        builder.HasIndex(e => new { e.Discriminator, e.ArticleId },
            "metrics_discriminator_article_index");

        builder.Property(e => e.ArticleId).HasColumnName("article_id");
    }
}
using Main.Entities.Analytics;
using Main.Entities.Sale.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class SaleContentAnalyticsRelationsConfiguration
	: IEntityTypeConfiguration<SaleContentAnalytics>
{
	public void Configure(EntityTypeBuilder<Main.Entities.Analytics.SaleContentAnalytics> builder)
		=> builder
			.HasOne<SaleContent>()
			.WithOne()
			.HasForeignKey<Main.Entities.Analytics.SaleContentAnalytics>(item => item.SaleContentId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("sale_content_analytics_sale_content_id_fk");
}

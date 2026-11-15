using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class SaleContentAnalyticsRelationsConfiguration
	: IEntityTypeConfiguration<Entities.Analytics.SaleContentAnalytics>
{
	public void Configure(EntityTypeBuilder<Entities.Analytics.SaleContentAnalytics> builder)
		=> builder
			.HasOne<Entities.Sale.SaleContent>()
			.WithOne()
			.HasForeignKey<Entities.Analytics.SaleContentAnalytics>(item => item.SaleContentId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("sale_content_analytics_sale_content_id_fk");
}

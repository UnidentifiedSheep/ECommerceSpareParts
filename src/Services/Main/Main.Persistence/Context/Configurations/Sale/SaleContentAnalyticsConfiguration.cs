using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Sale;

public sealed class SaleContentAnalyticsConfiguration : IEntityTypeConfiguration<SaleContentAnalytics>
{
	public void Configure(EntityTypeBuilder<SaleContentAnalytics> builder)
	{
		builder.ToTable("sale_content_analytics", "public");
		builder.HasKey(item => item.SaleContentId).HasName("sale_content_analytics_pk");
		builder.Property(item => item.SaleContentId).HasColumnName("sale_content_id").ValueGeneratedNever();
		builder.Property(item => item.OriginalAmountInBaseCurrency).HasColumnName("original_amount_in_base_currency");
		builder.Property(item => item.RevenueInBaseCurrency).HasColumnName("revenue_in_base_currency");
		builder.Property(item => item.CostInBaseCurrency).HasColumnName("cost_in_base_currency");
		builder.HasOne(item => item.Content).WithOne(item => item.Analytics).HasForeignKey<SaleContentAnalytics>(item => item.SaleContentId)
			.OnDelete(DeleteBehavior.Cascade).HasConstraintName("sale_content_analytics_sale_content_id_fk");
	}
}

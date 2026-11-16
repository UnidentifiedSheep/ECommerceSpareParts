using Main.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Analytics.Persistence.Configurations;

public sealed class SaleContentAnalyticsConfiguration : IEntityTypeConfiguration<SaleContentAnalytics>
{
	public void Configure(EntityTypeBuilder<SaleContentAnalytics> builder)
	{
		builder.ToTable("sale_content_analytics", "public");
		builder.HasKey(item => item.SaleContentId).HasName("sale_content_analytics_pk");
		builder.Property(item => item.SaleContentId).HasColumnName("sale_content_id").ValueGeneratedNever();
		builder.Property(item => item.SaleId).HasColumnName("sale_id");
		builder.Property(item => item.OrganizationId).HasColumnName("organization_id");
		builder.Property(item => item.BuyerId).HasColumnName("buyer_id");
		builder.Property(item => item.SaleDatetime).HasColumnName("sale_datetime");
		builder.Property(item => item.IsCompleted).HasColumnName("is_completed");
		builder.Property(item => item.Count).HasColumnName("count");
		builder.Property(item => item.OriginalAmountInBaseCurrency)
			.HasColumnName("original_amount_in_base_currency");
		builder.Property(item => item.RevenueInBaseCurrency).HasColumnName("revenue_in_base_currency");
		builder.Property(item => item.CostInBaseCurrency).HasColumnName("cost_in_base_currency");
		builder.HasIndex(item => item.SaleId, "sale_content_analytics_sale_id_index");
		builder.HasIndex(
			item => new
			{
				item.IsCompleted, item.SaleDatetime
			},
			"sale_content_analytics_is_completed_sale_datetime_index");
		builder.HasIndex(item => item.OrganizationId, "sale_content_analytics_organization_id_index");
		builder.HasIndex(item => item.BuyerId, "sale_content_analytics_buyer_id_index");
	}
}

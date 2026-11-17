using Main.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Analytics.Persistence.Configurations;

public sealed class AnalyticsRunConfiguration : IEntityTypeConfiguration<AnalyticsRun>
{
	public void Configure(EntityTypeBuilder<AnalyticsRun> builder)
	{
		builder.ToTable("analytics_runs", "public");
		builder.HasKey(item => item.Id).HasName("analytics_runs_pk");
		builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
		builder.Property(item => item.AnalysisType).HasColumnName("analysis_type").HasMaxLength(128);
		builder.Property(item => item.SchemaVersion).HasColumnName("schema_version");
		builder.Property(item => item.Parameters).HasColumnName("parameters").HasColumnType("jsonb");
		builder.Property(item => item.ParametersHash).HasColumnName("parameters_hash").HasMaxLength(64);
		builder.Property(item => item.PeriodStart).HasColumnName("period_start");
		builder.Property(item => item.PeriodEnd).HasColumnName("period_end");
		builder.Property(item => item.Summary).HasColumnName("summary").HasColumnType("jsonb");
		builder.Property(item => item.ItemsCount).HasColumnName("items_count");
		builder.Property(item => item.JobId).HasColumnName("job_id");
		builder.Property(item => item.CreatedBy).HasColumnName("created_by");
		builder.Property(item => item.IsPinned).HasColumnName("is_pinned");
		builder.HasIndex(item => item.JobId, "analytics_runs_job_id_index");
		builder.HasIndex(
			item => new
			{
				item.AnalysisType, item.ParametersHash
			},
			"analytics_runs_analysis_type_parameters_hash_index");
		builder.HasIndex(
			item => new
			{
				item.AnalysisType, item.PeriodEnd
			},
			"analytics_runs_analysis_type_period_end_index");
	}
}

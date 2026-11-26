using Main.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Analytics.Persistence.Configurations;

public sealed class AnalyticsRunItemConfiguration : IEntityTypeConfiguration<AnalyticsRunItem>
{
	public void Configure(EntityTypeBuilder<AnalyticsRunItem> builder)
	{
		builder.ToTable("analytics_run_items", "public");
		builder.HasKey(item => new
			{
				item.RunId, item.SubjectKey
			})
			.HasName("analytics_run_items_pk");
		builder.Property(item => item.RunId).HasColumnName("run_id");
		builder.Property(item => item.SubjectKey).HasColumnName("subject_key").HasMaxLength(64);
		builder.Property(item => item.Rank).HasColumnName("rank").IsRequired();
		builder.Property(item => item.Value).HasColumnName("value");
		builder.Property(item => item.Share).HasColumnName("share");
		builder.Property(item => item.Segments).HasColumnName("segments").HasColumnType("jsonb");
		builder.Property(item => item.Data).HasColumnName("data").HasColumnType("jsonb");
		builder.HasOne(item => item.Run)
			.WithMany(run => run.Items)
			.HasForeignKey(item => item.RunId)
			.HasConstraintName("analytics_run_items_run_id_fk")
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasIndex(
			item => new
			{
				item.RunId, item.Rank
			},
			"analytics_run_items_run_id_rank_index");
		builder.HasIndex(item => item.Segments, "analytics_run_items_segments_index")
			.HasMethod("gin")
			.HasOperators("jsonb_path_ops");
	}
}

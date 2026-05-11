using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities;

namespace Pricing.Persistence.Contexts.Configurations;

public class MarkupRangeConfiguration : IEntityTypeConfiguration<MarkupRange>
{
    public void Configure(EntityTypeBuilder<MarkupRange> builder)
    {
        builder.HasKey(e => e.Id)
            .HasName("markup_ranges_pk");

        builder.ToTable("markup_ranges");

        builder.HasIndex(e => e.GroupId, "IX_markup_ranges_group_id");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.GroupId)
            .HasColumnName("group_id");

        builder.Property(e => e.Markup)
            .HasColumnName("markup");

        builder.Property(e => e.RangeEnd)
            .HasColumnName("range_end");

        builder.Property(e => e.RangeStart)
            .HasColumnName("range_start");

        builder.HasOne(e => e.Group)
            .WithMany(e => e.MarkupRanges)
            .HasForeignKey(e => e.GroupId)
            .HasConstraintName("markup_ranges_markup_group_id_fk");
    }
}

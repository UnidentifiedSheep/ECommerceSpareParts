using Domain.CommonEntities.DataSlices.Definitions.Range;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class IntRangeDataSliceDefinitionConfiguration
    : IEntityTypeConfiguration<IntRangeDataSliceDefinition>
{
    public void Configure(EntityTypeBuilder<IntRangeDataSliceDefinition> builder)
    {
        builder.Property(x => x.RangeStart)
            .HasColumnName("int_range_start");

        builder.Property(x => x.RangeEnd)
            .HasColumnName("int_range_end");
    }
}

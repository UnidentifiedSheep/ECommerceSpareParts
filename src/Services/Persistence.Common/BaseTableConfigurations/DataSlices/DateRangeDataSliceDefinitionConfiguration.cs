using Domain.CommonEntities.DataSlices.Definitions.Range;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class DateRangeDataSliceDefinitionConfiguration
    : IEntityTypeConfiguration<DateRangeDataSliceDefinition>
{
    public void Configure(EntityTypeBuilder<DateRangeDataSliceDefinition> builder)
    {
        builder.Property(x => x.RangeStart)
            .HasColumnName("date_range_start");

        builder.Property(x => x.RangeEnd)
            .HasColumnName("date_range_end");
    }
}

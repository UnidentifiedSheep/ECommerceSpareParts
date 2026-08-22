using Domain.CommonEntities.DataSlices.Slices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class DateRangeDataSliceConfiguration
    : IEntityTypeConfiguration<DateRangeDataSlice>
{
    public void Configure(EntityTypeBuilder<DateRangeDataSlice> builder)
    {
        builder.Property(x => x.Value)
            .HasColumnName("date_value");
    }
}

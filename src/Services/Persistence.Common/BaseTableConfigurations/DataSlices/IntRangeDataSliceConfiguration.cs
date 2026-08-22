using Domain.CommonEntities.DataSlices.Slices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class IntRangeDataSliceConfiguration
    : IEntityTypeConfiguration<IntRangeDataSlice>
{
    public void Configure(EntityTypeBuilder<IntRangeDataSlice> builder)
    {
        builder.Property(x => x.Value)
            .HasColumnName("int_value");
    }
}

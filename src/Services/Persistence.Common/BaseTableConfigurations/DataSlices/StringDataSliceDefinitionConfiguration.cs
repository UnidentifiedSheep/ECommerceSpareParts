using Domain.CommonEntities.DataSlices.Definitions.Scalar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class StringDataSliceDefinitionConfiguration
    : IEntityTypeConfiguration<StringDataSliceDefinition>
{
    public void Configure(EntityTypeBuilder<StringDataSliceDefinition> builder)
    {
        builder.Property(x => x.Value)
            .HasColumnName("string_value");
    }
}

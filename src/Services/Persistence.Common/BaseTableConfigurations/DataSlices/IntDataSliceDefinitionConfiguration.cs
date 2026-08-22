using Domain.CommonEntities.DataSlices.Definitions.Scalar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class IntDataSliceDefinitionConfiguration
    : IEntityTypeConfiguration<IntDataSliceDefinition>
{
    public void Configure(EntityTypeBuilder<IntDataSliceDefinition> builder)
    {
        builder.Property(x => x.Value)
            .HasColumnName("int_value");
    }
}

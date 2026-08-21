using Domain.CommonEntities.DataSlices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class DataSliceGroupConfiguration : IEntityTypeConfiguration<DataSliceGroup>
{
    public void Configure(EntityTypeBuilder<DataSliceGroup> builder)
    {
        builder.ToTable("slices_group", "slices");

        builder.HasKey(x => x.Id)
            .HasName("group_pk");

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(e => e.SystemName)
            .HasColumnName("system_name")
            .HasMaxLength(DataSliceGroup.SystemNameMaxLength);

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(e => e.SystemName)
            .HasDatabaseName("slices_group_system_name_uindex")
            .IsUnique();

        builder.HasDiscriminator<string>("group_type");

        builder.Navigation(e => e.Definitions)
            .HasField("_definitions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

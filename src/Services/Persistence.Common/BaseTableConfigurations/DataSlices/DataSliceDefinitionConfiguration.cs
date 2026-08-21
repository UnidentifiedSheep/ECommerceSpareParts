using Domain.CommonEntities.DataSlices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class DataSliceDefinitionConfiguration : IEntityTypeConfiguration<DataSliceDefinition>
{
    public void Configure(EntityTypeBuilder<DataSliceDefinition> builder)
    {
        builder.ToTable("slices_definitions", "slices");

        builder.HasKey(x => x.Id)
            .HasName("slices_definition_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.DataSliceGroupId)
            .HasColumnName("slice_group_id");

        builder.Property(e => e.PublishedRevisionId)
            .HasColumnName("published_revision_id");

        builder.Property(e => e.PreparingRevisionId)
            .HasColumnName("preparing_revision_id");

        builder.Property(e => e.IsDirty)
            .HasColumnName("is_dirty");

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasOne(e => e.Group)
            .WithMany(e => e.Definitions)
            .HasForeignKey(e => e.DataSliceGroupId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("slices_definition_group_id_fk");

        builder.HasDiscriminator<string>("definition_type");

        builder.Navigation(e => e.Slices)
            .HasField("_slices")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

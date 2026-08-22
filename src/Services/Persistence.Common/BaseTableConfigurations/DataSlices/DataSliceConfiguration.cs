using Domain.CommonEntities.DataSlices;
using Domain.CommonEntities.DataSlices.Slices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations.DataSlices;

public sealed class DataSliceConfiguration : IEntityTypeConfiguration<DataSlice>
{
    public void Configure(EntityTypeBuilder<DataSlice> builder)
    {
        builder.ToTable("data_slices", "slices");

        builder.HasKey(x => x.Id)
            .HasName("data_slices_pk");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.DataSliceDefinitionId)
            .HasColumnName("slices_definition_id");

        builder.Property(e => e.RevisionId)
            .HasColumnName("revision_id");

        builder.Property(e => e.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb");

        builder.HasOne(e => e.Definition)
            .WithMany(e => e.Slices)
            .HasForeignKey(e => e.DataSliceDefinitionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("data_slices_definitions_fk");

        builder.HasIndex(e => new
            {
                e.DataSliceDefinitionId,
                e.RevisionId
            })
            .HasDatabaseName("data_slices_definition_revision_idx");

        builder.HasDiscriminator<string>("slice_type");
    }
}

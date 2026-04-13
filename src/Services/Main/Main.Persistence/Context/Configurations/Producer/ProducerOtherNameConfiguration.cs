using Main.Entities;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Producer;

public class ProducerOtherNameConfiguration : IEntityTypeConfiguration<ProducerOtherName>
{
    public void Configure(EntityTypeBuilder<ProducerOtherName> builder)
    {
        builder.ToTable("producers_other_names");
        
        builder.HasKey("producer_id", "other_name", "where_used")
            .HasName("producers_other_names_pk");

        builder.HasIndex(e => e.ProducerId)
            .HasDatabaseName("producers_other_names_producer_id_index");

        builder.HasIndex("other_name")
            .HasDatabaseName("producers_other_names_producer_other_name_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.WhereUsed)
            .HasDatabaseName("producers_other_names_where_used_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(e => e.ProducerId)
            .HasColumnName("producer_id");

        builder.OwnsOne(
            v => v.OtherName,
            b =>
            {
                b.Property(x => x.Value)
                    .HasMaxLength(64)
                    .HasColumnName("other_name");
            });
        
        builder.Property(e => e.WhereUsed)
            .HasMaxLength(64)
            .HasColumnName("where_used");

        builder.HasOne<Entities.Producer.Producer>()
            .WithMany()
            .HasForeignKey(d => d.ProducerId)
            .HasConstraintName("producers_other_names_producer_id_fk");
    }
}
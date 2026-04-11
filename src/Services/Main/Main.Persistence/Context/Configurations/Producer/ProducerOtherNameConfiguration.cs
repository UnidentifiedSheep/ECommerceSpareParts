using Main.Entities;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Producer;

public class ProducerOtherNameConfiguration : IEntityTypeConfiguration<ProducersOtherName>
{
    public void Configure(EntityTypeBuilder<ProducersOtherName> builder)
    {
        builder.ToTable("producers_other_names");
        
        builder.HasKey(e => new { e.ProducerId, e.ProducerOtherName, e.WhereUsed })
            .HasName("producers_other_names_pk");

        builder.HasIndex(e => e.ProducerId)
            .HasDatabaseName("producers_other_names_producer_id_index");

        builder.HasIndex(e => e.ProducerOtherName)
            .HasDatabaseName("producers_other_names_producer_other_name_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.WhereUsed)
            .HasDatabaseName("producers_other_names_where_used_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(e => e.ProducerId)
            .HasColumnName("producer_id");
        
        builder.Property(e => e.ProducerOtherName)
            .HasMaxLength(64)
            .HasColumnName("producer_other_name");
        
        builder.Property(e => e.WhereUsed)
            .HasMaxLength(64)
            .HasColumnName("where_used");

        builder.HasOne<Entities.Producer.Producer>()
            .WithMany()
            .HasForeignKey(d => d.ProducerId)
            .HasConstraintName("producers_other_names_producer_id_fk");
    }
}
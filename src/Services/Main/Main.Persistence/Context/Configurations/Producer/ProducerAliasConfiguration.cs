using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Producer;

public class ProducerAliasConfiguration : IEntityTypeConfiguration<ProducerAlias>
{
    public void Configure(EntityTypeBuilder<ProducerAlias> builder)
    {
        builder.ToTable("producers_aliases", "public");

        builder.HasKey(e => e.Alias)
            .HasName("producers_other_names_pk");

        builder.HasIndex(e => e.ProducerId)
            .HasDatabaseName("producers_other_names_producer_id_index");

        builder.HasIndex(e => e.Alias)
            .HasDatabaseName("producers_other_names_producer_other_name_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(e => e.ProducerId)
            .HasColumnName("producer_id");

        builder.Property(e => e.Alias)
            .HasMaxLength(64)
            .HasColumnName("other_name");

        builder.HasOne<Entities.Producer.Producer>()
            .WithMany(x => x.Aliases)
            .HasForeignKey(d => d.ProducerId)
            .HasConstraintName("producers_other_names_producer_id_fk");
    }
}
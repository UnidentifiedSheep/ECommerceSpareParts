using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Producer;

public class ProducerSupplierMappingConfiguration : IEntityTypeConfiguration<ProducerSupplierMapping>
{
    public void Configure(EntityTypeBuilder<ProducerSupplierMapping> builder)
    {
        builder.ToTable("producer_supplier_mappings", "public");

        builder.HasKey(e => e.Id)
            .HasName("producer_supplier_mappings_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Supplier)
            .HasColumnName("supplier")
            .HasMaxLength(32);

        builder.Property(e => e.ProducerId)
            .HasColumnName("producer_id");

        builder.Property(e => e.SupplierProducerName)
            .HasColumnName("producer_supplier_name")
            .HasMaxLength(128);

        builder.HasAlternateKey(e => new
            {
                e.SupplierProducerName,
                e.Supplier
            })
            .HasName("producer_supplier_mappings_supplier_name_key");

        builder.HasOne<Entities.Producer.Producer>()
            .WithMany(e => e.SupplierMappings)
            .HasForeignKey(d => d.ProducerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("producer_supplier_mappings_producer_id_fk");
    }
}

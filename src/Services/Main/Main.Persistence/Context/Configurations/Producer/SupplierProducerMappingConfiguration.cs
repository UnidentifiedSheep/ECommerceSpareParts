using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Producer;

public class SupplierProducerMappingConfiguration : IEntityTypeConfiguration<SupplierProducerMapping>
{
    public void Configure(EntityTypeBuilder<SupplierProducerMapping> builder)
    {
        builder.ToTable("supplier_producer_mappings", "public");

        builder.HasKey(e => e.Id)
            .HasName("supplier_producer_mappings_pk");
        
        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.Supplier)
            .HasColumnName("supplier")
            .HasMaxLength(32);
        
        builder.Property(e => e.ProducerId)
            .HasColumnName("producer_id");
        
        builder.Property(e => e.SupplierProducerName)
            .HasColumnName("supplier_producer_name")
            .HasMaxLength(128);

        builder.HasIndex(e => new { e.ProducerId, e.Supplier })
            .HasDatabaseName("supplier_producer_mappings_uidx")
            .IsUnique();
        
        builder.HasOne<Entities.Producer.Producer>()
            .WithMany()
            .HasForeignKey(d => d.ProducerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("supplier_producer_mappings_producer_id_fk");
    }
}
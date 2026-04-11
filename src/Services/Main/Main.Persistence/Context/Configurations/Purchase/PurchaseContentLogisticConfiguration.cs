using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Purchase;

public class PurchaseContentLogisticConfiguration : IEntityTypeConfiguration<PurchaseContentLogistic>
{
    public void Configure(EntityTypeBuilder<PurchaseContentLogistic> builder)
    {
        builder.ToTable("purchase_content_logistics");
        
        builder.HasKey(e => e.PurchaseContentId)
            .HasName("purchase_content_logistics_pk");

        builder.Property(e => e.PurchaseContentId)
            .ValueGeneratedNever()
            .HasColumnName("purchase_content_id");
        
        builder.Property(e => e.AreaM3)
            .HasColumnName("area_m3");
        
        builder.Property(e => e.Price)
            .HasColumnName("price");
        
        builder.Property(e => e.WeightKg)
            .HasColumnName("weight_kg");

        builder.HasOne<PurchaseContent>()
            .WithOne(p => p.PurchaseContentLogistic)
            .HasForeignKey<PurchaseContentLogistic>(d => d.PurchaseContentId)
            .HasConstraintName("purchase_content_logistics_purchase_content_id_fk");
    }
}
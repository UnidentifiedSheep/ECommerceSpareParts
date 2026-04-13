using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Producer;

public class ProducerConfiguration : IEntityTypeConfiguration<Entities.Producer.Producer>
{
    public void Configure(EntityTypeBuilder<Entities.Producer.Producer> builder)
    {
        builder.ToTable("producer");
        
        builder.HasKey(e => e.Id)
            .HasName("producer_id");

        builder.HasIndex("name")
            .HasDatabaseName("producer_name_uindex")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(500);
        
        builder.Property(e => e.ImagePath)
            .HasMaxLength(255)
            .HasColumnName("image_path");

        builder.OwnsOne(b => b.Name,
            b =>
            {
                b.Property(x => x.Value)
                    .HasMaxLength(64)
                    .HasColumnName("name");
            });
    }
}
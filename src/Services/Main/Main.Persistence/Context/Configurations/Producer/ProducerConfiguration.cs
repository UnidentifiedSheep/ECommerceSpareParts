using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Producer;

public class ProducerConfiguration : IEntityTypeConfiguration<Entities.Producer>
{
    public void Configure(EntityTypeBuilder<Entities.Producer> builder)
    {
        builder.ToTable("producer");
        
        builder.HasKey(e => e.Id)
            .HasName("producer_id");

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("producer_name_uindex")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.Description)
            .HasColumnName("description");
        
        builder.Property(e => e.ImagePath)
            .HasMaxLength(255)
            .HasColumnName("image_path");
        
        builder.Property(e => e.IsOe)
            .HasColumnName("is_oe");
        
        builder.Property(e => e.Name)
            .HasMaxLength(64)
            .HasColumnName("name");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Event;

public class EventConfiguration : IEntityTypeConfiguration<Entities.Event.Event>
{
    public void Configure(EntityTypeBuilder<Entities.Event.Event> builder)
    {
        builder.ToTable("events");
        
        builder.HasIndex(e => e.Discriminator)
            .HasDatabaseName("event_discriminator_idx");
        
        builder.HasKey(e => e.Id)
            .HasName("events_id_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Json)
            .HasColumnName("json")
            .HasColumnType("jsonb");
        
        builder.Property(e => e.Discriminator)
            .HasColumnName("discriminator");
        
        builder.HasDiscriminator(e => e.Discriminator);
    }
}
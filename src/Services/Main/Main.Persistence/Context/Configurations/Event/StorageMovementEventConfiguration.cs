using Main.Entities.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Event;

public class StorageMovementEventConfiguration : IEntityTypeConfiguration<StorageMovementEvent>
{
    public void Configure(EntityTypeBuilder<StorageMovementEvent> builder)
    {
        
    }
}
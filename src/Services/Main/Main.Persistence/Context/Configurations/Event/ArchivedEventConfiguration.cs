using Main.Entities.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Event;

public sealed class ArchivedEventConfiguration : IEntityTypeConfiguration<ArchivedEvent>
{
	public void Configure(EntityTypeBuilder<ArchivedEvent> builder) => builder.HasBaseType<Entities.Event.Event>();
}

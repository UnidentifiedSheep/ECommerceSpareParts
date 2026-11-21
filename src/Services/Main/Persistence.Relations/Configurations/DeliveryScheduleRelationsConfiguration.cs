using Main.Entities.Logistics.Schedule;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class DeliveryScheduleRelationsConfiguration : IEntityTypeConfiguration<DeliverySchedule>
{
	public void Configure(EntityTypeBuilder<DeliverySchedule> builder)
		=> builder
			.HasOne<Storage>()
			.WithMany()
			.HasForeignKey(schedule => schedule.StorageCode)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("delivery_schedules_storage_code_fk");
}

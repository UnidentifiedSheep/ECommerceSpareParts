using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Main.Modules.Logistics.Persistence.Configurations.Schedule;

public sealed class DeliveryScheduleConfiguration : IEntityTypeConfiguration<DeliverySchedule>
{
	public void Configure(EntityTypeBuilder<DeliverySchedule> builder)
	{
		builder.ToTable("delivery_schedules", "logistics");

		builder
			.HasKey(schedule => new { schedule.Supplier, schedule.StorageCode })
			.HasName("delivery_schedules_pk");

		builder.Property(schedule => schedule.Supplier).HasColumnName("supplier").HasMaxLength(32);

		builder
			.Property(schedule => schedule.StorageCode)
			.HasColumnName("storage_code")
			.HasMaxLength(DeliverySchedule.StorageCodeMaxLength);

		builder
			.Property(schedule => schedule.Days)
			.HasColumnName("days")
			.HasConversion(new EnumToNumberConverter<DeliveryDays, int>());

		builder.Property(schedule => schedule.ArrivalTime).HasColumnName("arrival_time");

		builder.HasIndex(schedule => schedule.StorageCode).HasDatabaseName("delivery_schedules_storage_code_index");

		builder
			.HasMany(schedule => schedule.Exceptions)
			.WithOne()
			.HasForeignKey(exception => new { exception.Supplier, exception.StorageCode })
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("delivery_schedule_exceptions_delivery_schedules_fk");

		builder
			.Navigation(schedule => schedule.Exceptions)
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}

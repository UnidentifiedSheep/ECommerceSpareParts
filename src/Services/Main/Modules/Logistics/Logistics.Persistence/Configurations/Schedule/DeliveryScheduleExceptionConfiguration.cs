using Main.Entities.Logistics.Schedule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Logistics.Persistence.Configurations.Schedule;

public sealed class DeliveryScheduleExceptionConfiguration : IEntityTypeConfiguration<DeliveryScheduleException>
{
	public void Configure(EntityTypeBuilder<DeliveryScheduleException> builder)
	{
		builder.ToTable("delivery_schedule_exceptions", "logistics");

		builder
			.HasKey(exception => new { exception.Supplier, exception.StorageCode, exception.Date })
			.HasName("delivery_schedule_exceptions_pk");

		builder.Property(exception => exception.Supplier).HasColumnName("supplier").HasMaxLength(32);

		builder
			.Property(exception => exception.StorageCode)
			.HasColumnName("storage_code")
			.HasMaxLength(DeliverySchedule.StorageCodeMaxLength);

		builder.Property(exception => exception.Date).HasColumnName("date");

		builder.HasIndex(exception => exception.Date).HasDatabaseName("delivery_schedule_exceptions_date_index");
	}
}

using Main.Entities.Event;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Event;

public class ReservationManualChangeEventConfiguration : IEntityTypeConfiguration<ReservationManualChangeEvent>
{
    public void Configure(EntityTypeBuilder<ReservationManualChangeEvent> builder)
    {
        builder.Metadata.SetDiscriminatorValue("ReservationManualChangeEvent");
        
        builder.Property(e => e.ReservationId)
            .HasColumnName("reservation_id");
        
        builder.HasIndex(e => e.ReservationId)
            .HasDatabaseName("reservation_manual_change_event_reservation_id_idx");
        
        builder.HasOne<StorageContentReservation>()
            .WithMany()
            .HasForeignKey(e => e.ReservationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("reservation_manual_change_event_reservation_id_fk");
    }
}
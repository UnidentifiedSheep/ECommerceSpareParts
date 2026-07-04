using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserVehicleConfiguration : IEntityTypeConfiguration<UserVehicle>
{
    public void Configure(EntityTypeBuilder<UserVehicle> builder)
    {
        builder.ToTable("user_vehicles", "public");

        builder.HasKey(e => e.Id)
            .HasName("user_vehicles_pk");

        builder.HasIndex(e => e.Comment)
            .HasDatabaseName("user_vehicles_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.PlateNumber)
            .HasDatabaseName("user_vehicles_plate_number_uindex")
            .IsUnique();

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("user_vehicles_user_id_index");

        builder.HasIndex(e => e.VehicleId)
            .HasDatabaseName("user_vehicles_vehicle_id_index");

        builder.HasIndex(e => e.Vin)
            .HasDatabaseName("user_vehicles_vin_uindex")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Comment)
            .HasMaxLength(500)
            .HasColumnName("comment");

        builder.Property(e => e.PlateNumber)
            .HasMaxLength(32)
            .HasColumnName("plate_number");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.VehicleId)
            .HasColumnName("vehicle_id");

        builder.Property(e => e.Vin)
            .HasMaxLength(50)
            .HasColumnName("vin");

        builder.HasOne(e => e.User)
            .WithMany(p => p.Vehicles)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_vehicles_users_id_fk");
    }
}
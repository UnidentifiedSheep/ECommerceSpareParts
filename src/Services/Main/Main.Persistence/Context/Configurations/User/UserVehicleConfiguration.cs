using Main.Entities;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserVehicleConfiguration : IEntityTypeConfiguration<UserVehicle>
{
    public void Configure(EntityTypeBuilder<UserVehicle> builder)
    {
        builder.ToTable("user_vehicles");
        
        builder.HasKey(e => e.Id)
            .HasName("user_vehicles_pk");

        builder.HasIndex(e => e.Comment)
            .HasDatabaseName("user_vehicles_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Manufacture)
            .HasDatabaseName("user_vehicles_manufacture_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Model)
            .HasDatabaseName("user_vehicles_model_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.PlateNumber)
            .HasDatabaseName("user_vehicles_plate_number_uindex")
            .IsUnique();

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("user_vehicles_user_id_index");

        builder.HasIndex(e => e.Vin)
            .HasDatabaseName("user_vehicles_vin_uindex")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.Comment)
            .HasColumnName("comment");

        builder.Property(e => e.EngineCode)
            .HasColumnName("engine_code");
        
        builder.Property(e => e.Manufacture)
            .HasMaxLength(50)
            .HasColumnName("manufacture");
        
        builder.Property(e => e.Model)
            .HasMaxLength(125)
            .HasColumnName("model");
        
        builder.Property(e => e.Modification)
            .HasColumnName("modification");
        
        builder.Property(e => e.PlateNumber)
            .HasColumnName("plate_number");
        
        builder.Property(e => e.ProductionYear)
            .HasColumnName("production_year");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        
        builder.Property(e => e.Vin)
            .HasMaxLength(50)
            .HasColumnName("vin");

        builder.HasOne<Entities.User.User>()
            .WithMany(p => p.UserVehicles)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_vehicles_users_id_fk");
    }
}
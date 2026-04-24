using Main.Entities;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Auth;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("user_permissions", "auth");
        
        builder.HasKey(e => new { e.UserId, e.Permission }).HasName("user_permissions_pk");


        builder.HasIndex(e => e.Permission)
            .HasDatabaseName("IX_user_permissions_permission");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        
        builder.Property(e => e.Permission)
            .HasColumnName("permission");

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(d => d.Permission)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_permissions_permissions_name_fk");

        builder.HasOne<Entities.User.User>()
            .WithMany(p => p.Permissions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("user_permissions_users_id_fk");
    }
}
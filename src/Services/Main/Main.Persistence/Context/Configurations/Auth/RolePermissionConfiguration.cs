using Main.Entities;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Auth;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "auth");

        builder.HasKey(x => new { x.RoleName, x.PermissionName })
            .HasName("role_permissions_pk");

        builder.Property(x => x.RoleName)
            .HasColumnName("role");

        builder.Property(x => x.PermissionName)
            .HasColumnName("permission");

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionName)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("role_permissions_permissions_name_fk");
        
        builder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleName)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_permissions_roles_id_fk");

        builder.HasIndex(x => x.PermissionName)
            .HasDatabaseName("IX_role_permissions_permission_name");
    }
}
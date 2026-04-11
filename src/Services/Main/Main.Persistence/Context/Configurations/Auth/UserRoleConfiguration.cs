using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.User;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "auth");
        
        builder.HasKey(e => new { e.UserId, e.RoleName })
            .HasName("user_roles_pk");

        builder.HasIndex(e => e.RoleName)
            .HasDatabaseName("IX_user_roles_role_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        
        builder.Property(e => e.RoleName)
            .HasColumnName("role_name");

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(d => d.RoleName)
            .HasConstraintName("user_roles_roles_name_fk");

        builder.HasOne<Entities.User>()
            .WithMany(p => p.UserRoles)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("user_roles_users_id_fk");
    }
}
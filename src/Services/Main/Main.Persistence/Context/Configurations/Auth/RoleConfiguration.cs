using Main.Entities.Auth;
using Main.Entities.Auth.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Auth;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", "auth");

        builder.HasKey(e => e.Name);

        builder.Property(e => e.Name)
            .HasConversion(
                v => v.Value,
                v => new RoleName(v)
            )
            .HasMaxLength(24)
            .HasColumnName("normalized_name");

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("description");

        builder.Navigation(e => e.RolePermissions)
            .HasField("_rolePermissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
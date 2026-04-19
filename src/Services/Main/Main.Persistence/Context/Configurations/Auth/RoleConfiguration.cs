using Main.Entities;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Auth;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", "auth");

        builder.OwnsOne(
            e => e.Name,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("name")
                    .HasMaxLength(24);
                
                b.Property(e => e.NormalizedValue)
                    .HasColumnName("normalized_name")
                    .HasMaxLength(24);
            });
        
        builder
            .HasKey("normalized_name")
            .HasName("roles_pk");
        
        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("description");
        
        builder.Navigation(e => e.RolePermissions)
            .HasField("_rolePermissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
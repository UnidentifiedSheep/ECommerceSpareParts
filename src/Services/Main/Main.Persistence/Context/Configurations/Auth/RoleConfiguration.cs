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
        
        builder
            .HasKey(e => e.NormalizedName)
            .HasName("roles_pk");
        
        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("description");
            
        builder.Property(e => e.Name)
            .HasMaxLength(24)
            .HasColumnName("name");
        builder.Property(e => e.NormalizedName)
            .HasMaxLength(24)
            .HasColumnName("normalized_name");
    }
}
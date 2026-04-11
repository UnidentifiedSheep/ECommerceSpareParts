using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Auth;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions", "auth");
        
        builder.HasKey(e => e.Name)
            .HasName("permissions_pk");

        builder.Property(e => e.Name)
            .HasColumnName("name");
        
        builder.Property(e => e.Description)
            .HasColumnName("description");
    }
}
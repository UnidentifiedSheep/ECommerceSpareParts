using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Organization;

public class OrganizationConfiguration : IEntityTypeConfiguration<Entities.Organization.Organization>
{
    public void Configure(EntityTypeBuilder<Entities.Organization.Organization> builder)
    {
        builder.ToTable("organizations", "auth");

        builder.HasKey(x => x.Id)
            .HasName("organizations_pk");

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.SystemName)
            .HasMaxLength(128)
            .HasColumnName("system_name");

        builder.HasIndex(x => x.SystemName)
            .HasDatabaseName("organizations_system_name_uindex")
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .HasColumnName("name");

        builder.Property(x => x.Type)
            .HasMaxLength(32)
            .HasColumnName("type");

        builder.Navigation(x => x.Members)
            .HasField("_members")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

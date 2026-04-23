using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.BaseTableConfigurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("settings");
        
        builder.HasKey(e => e.Key)
            .HasName("settings_pk");

        builder.Property(e => e.Key)
            .HasColumnName("key") ;

        builder.HasDiscriminator(e => e.Key);
        
        builder.Property(e => e.Json)
            .HasColumnName("json")
            .HasColumnType("jsonb");
    }
}
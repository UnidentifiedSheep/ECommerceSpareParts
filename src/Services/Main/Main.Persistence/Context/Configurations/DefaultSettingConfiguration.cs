using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public class DefaultSettingConfiguration : IEntityTypeConfiguration<DefaultSetting>
{
    public void Configure(EntityTypeBuilder<DefaultSetting> builder)
    {
        builder.ToTable("default_settings");
        
        builder.HasKey(e => e.Key)
            .HasName("default_settings_pk");

        builder.Property(e => e.Key)
            .HasColumnName("key");
        
        builder.Property(e => e.Value)
            .HasColumnName("value");
    }
}
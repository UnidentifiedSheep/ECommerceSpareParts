using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public class CoefficientConfiguration : IEntityTypeConfiguration<Coefficient>
{
    public void Configure(EntityTypeBuilder<Coefficient> builder)
    {
        builder.ToTable("coefficients");
        
        builder.HasKey(e => e.Name)
            .HasName("coefficients_pk");

        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .HasColumnName("name");
        
        builder.Property(e => e.Order)
            .HasColumnName("order");
        
        builder.Property(e => e.Type)
            .HasMaxLength(56)
            .HasColumnName("type");
        
        builder.Property(e => e.Value)
            .HasColumnName("value");
    }
}
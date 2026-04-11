using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AllDateTimesToUtc(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var dateTimeProps = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

            foreach (var prop in dateTimeProps)
                prop.SetValueConverter(
                    new ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                        v => v.ToUniversalTime()
                    )
                );
        }

        return modelBuilder;
    }

    public static ModelBuilder AllEnumsToString(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var enumProperties = entityType.ClrType
                .GetProperties()
                .Where(p => p.PropertyType.IsEnum);

            foreach (var property in enumProperties)
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion<string>();
        }

        return modelBuilder;
    }

    public static ModelBuilder AddFieldsForAuditableEntities(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType)) continue;
            
            var builder = modelBuilder.Entity(entityType.ClrType);

            builder.Property(nameof(IAuditable.CreatedAt))
                .HasColumnName("created_at");

            builder.Property(nameof(IAuditable.UpdatedAt))
                .HasColumnName("updated_at");
        }
        return modelBuilder;
    }
}
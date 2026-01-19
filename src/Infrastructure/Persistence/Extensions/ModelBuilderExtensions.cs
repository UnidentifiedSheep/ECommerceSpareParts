using Microsoft.EntityFrameworkCore;

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
            {
                prop.SetValueConverter(
                    new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                        v => v.ToUniversalTime()
                    )
                );
            }
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
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion<string>();
            }
        }
        return modelBuilder;
    }
}
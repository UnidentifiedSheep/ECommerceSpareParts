using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Common;

public static class MetadataModelBuilderExtensions
{
	public static ModelBuilder ApplyMetadataConfigurations(this ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToArray())
		{
			var metadataInterface = entityType.ClrType.GetInterfaces()
				.SingleOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IMetadata<>));
			if (metadataInterface is null ||
				(entityType.BaseType is { } baseType && metadataInterface.IsAssignableFrom(baseType.ClrType)))
				continue;

			modelBuilder.Entity(entityType.ClrType)
				.ComplexProperty(metadataInterface.GenericTypeArguments[0], nameof(IMetadata<object>.Metadata))
				.IsRequired()
				.ToJson("metadata");
		}

		return modelBuilder;
	}
}

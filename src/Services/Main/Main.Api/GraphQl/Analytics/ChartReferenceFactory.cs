using Main.Api.GraphQl.Types.Organization;
using Main.Api.GraphQl.Types.Product;
using Main.Api.GraphQl.Types.User;
using Main.Entities.Analytics.Exceptions;
using Main.Modules.Analytics.Abstractions.Models;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Api.GraphQl.Analytics;

public static class ChartReferenceFactory
{
	public static IReadOnlySet<SchemaEntity> SupportedEntities { get; } = new HashSet<SchemaEntity>
	{
		SchemaEntity.Product,
		SchemaEntity.Organization,
		SchemaEntity.User
	};

	public static object Create(ChartReference reference)
		=> reference.Entity switch
		{
			SchemaEntity.Product => new GqlProduct(Key<int>(reference)),
			SchemaEntity.Organization => new GqlOrganization(Key<Guid>(reference)),
			SchemaEntity.User => new GqlUser(Key<Guid>(reference)),
			_ => throw new NotSupportedException($"Chart reference entity '{reference.Entity}' is not supported.")
		};

	private static T Key<T>(ChartReference reference) where T : struct
		=> reference.Key is T key
			? key
			: throw new ChartDataPointReferenceInvalidException(reference.Field, reference.Entity.ToString());
}

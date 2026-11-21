using HotChocolate.Types.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Common.Types.Sorting;

public static class SortTypeNames
{
	public static string Create(IDescriptorContext context, Type entityType, string suffix)
		=> $"{context.Services.GetService<SortTypeNamingOptions>()?.Prefix}{entityType.Name}{suffix}";
}

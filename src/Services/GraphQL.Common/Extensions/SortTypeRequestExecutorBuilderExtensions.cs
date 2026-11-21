using GraphQL.Common.Types.Sorting;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Common.Extensions;

public static class SortTypeRequestExecutorBuilderExtensions
{
	public static IRequestExecutorBuilder AddSortTypePrefix(this IRequestExecutorBuilder builder, string prefix)
		=> builder.ConfigureSchemaServices(services => services.AddSingleton(new SortTypeNamingOptions(prefix)));
}

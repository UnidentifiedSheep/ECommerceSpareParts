using GraphQL.Common.ErrorFilters;
using GraphQL.Common.Types;
using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GraphQL.Common.Extensions;

public static class ServiceCollectionExtensions
{
	public static IRequestExecutorBuilder AddCommonGraphQl(this IServiceCollection services, string name)
	{
		services.AddHttpContextAccessor();

		return services
			.AddGraphQLServer(name)
			.ModifyServerOptions(options =>
			{
				options.Batching = AllowedBatching.All;
			})
			.AddCommonAuthorization()
			.AddApplicationService<ILoggerFactory>()
			.AddApplicationService<IContextualStringLocalizer>()
			.AddApplicationService<IHttpContextAccessor>()
			.AddErrorFilter<ValidationErrorFilter>()
			.AddErrorFilter<DbValidationErrorFilter>()
			.AddErrorFilter<AnyErrorFilter>()
			.AddType<GqlPagination>()
			.AddType<GqlSortBy>();
	}
}

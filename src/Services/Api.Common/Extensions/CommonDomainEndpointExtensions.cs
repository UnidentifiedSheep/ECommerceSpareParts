using Application.Common.Interfaces.Domains;

namespace Api.Common.Extensions;

public static class CommonDomainEndpointExtensions
{
	public static bool HasCommonDomain<TDomain>(this IEndpointRouteBuilder endpoints)
		where TDomain : ICommonDomain =>
		endpoints.ServiceProvider.GetService<ICommonDomainMarker<TDomain>>() is not null;
}

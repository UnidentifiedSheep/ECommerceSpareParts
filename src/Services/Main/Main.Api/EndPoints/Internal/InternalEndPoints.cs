using Carter;
using Main.Api.Settings;

namespace Main.Api.EndPoints.Internal;

public class InternalEndPoints : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
		=> app
			.MapGroup("/internal")
			.WithTags("Internal")
			.AddInternalAuthEndPoints()
			.AddInternalProducerEndPoints()
			.AddInternalProductsEndPoints()
			.AddInternalCurrencyEndPoints()
			.AddInternalSettingEndPoints();
}

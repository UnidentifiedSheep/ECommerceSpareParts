using Carter;

namespace Api.Common.EndPoints.Internal;

public class InternalEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/internal")
            .WithTags("Internal")
            .AddInternalSettingEndPoints();
    }
}

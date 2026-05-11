using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ProductWeight.DeleteProductWeight;
using MediatR;

namespace Main.Api.EndPoints.ProductWeight;

public class DeleteProductWeightEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id:int}/weights", async (ISender sender, int id, CancellationToken token) =>
            {
                var command = new DeleteProductWeightCommand(id);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("Article Weight")
            .WithDescription("Удаление веса артикула.")
            .WithDisplayName("Удаление веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_DELETE);
    }
}
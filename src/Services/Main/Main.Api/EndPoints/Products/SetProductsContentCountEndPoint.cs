using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ProductContent.SetProductContentQuantity;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record SetProductsContentCountRequest(int Count);

public class SetProductsContentCountEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{productId}/contents/{childProductId}", async (
                ISender sender,
                int productId,
                int childProductId,
                SetProductsContentCountRequest request,
                CancellationToken token) =>
            {
                var command = new SetProductsContentCountCommand(productId, childProductId, request.Count);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("Articles")
            .WithName("Установка входящего количества в содержимое артикула")
            .RequireAnyPermission("ARTICLE.CONTENT.EDIT");
    }
}
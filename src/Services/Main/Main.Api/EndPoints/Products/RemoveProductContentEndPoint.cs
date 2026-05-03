using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ProductContent.RemoveProductContent;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public class RemoveProductContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{productId}/contents/{childProductId}",
                async (ISender sender, int productId, int childProductId, CancellationToken token) =>
                {
                    var command = new RemoveProductContentCommand(productId, childProductId);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).WithTags("Articles")
            .WithDescription("Удаление содержимого артикула в бд")
            .WithDisplayName("Удаление содержимого артикула")
            .RequireAnyPermission("ARTICLE.CONTENT.DELETE");
    }
}
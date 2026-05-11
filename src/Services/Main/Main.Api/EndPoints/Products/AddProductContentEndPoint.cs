using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ProductContent.AddProductContent;
using MediatR;

namespace Main.Api.EndPoints.Products;

public record AddProductContentRequest(Dictionary<int, int> Content);

public class AddProductContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/Products/{productId}/contents", async (
                ISender sender,
                int productId,
                AddProductContentRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new AddProductContentCommand(productId, request.Content);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Articles")
            .WithDescription("Добавление содержимое артикула")
            .WithDisplayName("Добавление содержимое артикула")
            .Accepts<AddProductContentRequest>(false, "application/json")
            .RequireAnyPermission("ARTICLE.CONTENT.CREATE");
    }
}
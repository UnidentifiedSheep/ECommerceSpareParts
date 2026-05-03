using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ProductSizes.SetProductSizes;
using MediatR;

namespace Main.Api.EndPoints.ArticleSize;

public record PutProductSizeRequest(decimal Length, decimal Width, decimal Height, DimensionUnit Unit);

public class PutProductSizeEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:int}/sizes", async (
                ISender sender,
                int id,
                PutProductSizeRequest request,
                CancellationToken token) =>
            {
                var command =
                    new SetProductSizesCommand(id, request.Length, request.Width, request.Height, request.Unit);
                await sender.Send(command, token);
                return Results.Created();
            }).WithTags("Article Size")
            .WithDescription("Установка размеров артикула.")
            .WithDisplayName("Установка размеров артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_CREATE);
    }
}
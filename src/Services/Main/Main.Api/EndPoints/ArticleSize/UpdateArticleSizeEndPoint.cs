using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ArticleSizes.SetArticleSizes;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleSize;

public record UpdateArticleSizeRequest(decimal Length, decimal Width, decimal Height, DimensionUnit Unit);

public class UpdateArticleSizeEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/articles/{id:int}/sizes", async (ISender sender, int id, 
                UpdateArticleSizeRequest request, CancellationToken token) =>
            {
                var command = new SetArticleSizesCommand(id, request.Length, request.Width, request.Height, request.Unit);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("Article Size")
            .WithDescription("Обновление размеров артикула.")
            .WithDisplayName("Обновление размеров артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_UPDATE);
    }
}
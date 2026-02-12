using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ArticleSizes.SetArticleSizes;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleSize;

public record CreateArticleSizeRequest(decimal Length, decimal Width, decimal Height, DimensionUnit Unit);

public class CreateArticleSizeEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{id:int}/sizes", async (ISender sender, int id, 
            CreateArticleSizeRequest request, CancellationToken token) =>
        {
            var command = new SetArticleSizesCommand(id, request.Length, request.Width, request.Height, request.Unit);
            await sender.Send(command, token);
            return Results.Created();
        }).WithTags("Article Size")
        .WithDescription("Установка размеров артикула.")
        .WithDisplayName("Установка размеров артикула.")
        .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_CREATE);
    }
}
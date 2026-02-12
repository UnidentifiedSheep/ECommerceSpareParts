using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ArticleSizes.DeleteArticleSizes;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleSize;

public class DeleteArticleSizeEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/{id:int}/sizes", async (ISender sender, int id, CancellationToken token) =>
        {
            await sender.Send(new DeleteArticleSizesCommand(id), token);
            return Results.NoContent();
        }).WithTags("Article Size")
        .WithDescription("Удаление размеров артикула.")
        .WithDisplayName("Удаление размеров артикула.")
        .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_DELETE);
    }
}
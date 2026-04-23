using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductContent.GetProductContents;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record GetArticleContentResponse(IEnumerable<ProductContentDto> Content);

public class GetArticleContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId}/contents", async (ISender sender, int articleId, CancellationToken token) =>
            {
                var result = await sender.Send(new GetProductContentsQuery(articleId), token);
                var response = result.Adapt<GetArticleContentResponse>();
                return Results.Ok(response);
            }).WithName("получить содержание артикула")
            .WithTags("Articles")
            .WithDescription("Получить содержимое артикула по id.")
            .Produces<GetArticleContentResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("получить содержание артикула");
    }
}
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.ChangeArticleLinkage;

public record ChangeArticleLinkageRequest(IEnumerable<NewArticleLinkageDto> Linkages);

public class ChangeArticleLinkageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/crosses", async (ISender sender, ChangeArticleLinkageRequest request, CancellationToken token) =>
        {
            var command = request.Adapt<ChangeArticleLinkageCommand>();
            await sender.Send(command, token);
            return Results.Ok();
        }).RequireAuthorization("AMW")
        .WithGroup("Articles")
        .WithDescription("Создание кроссировки между артикулами")
        .WithDisplayName("Создание кроссировки");
    }
}
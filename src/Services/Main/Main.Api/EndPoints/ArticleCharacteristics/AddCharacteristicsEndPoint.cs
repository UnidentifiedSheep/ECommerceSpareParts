using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ArticleCharacteristics.AddCharacteristics;
using Main.Core.Dtos.Amw.ArticleCharacteristics;
using MediatR;

namespace Main.Api.EndPoints.ArticleCharacteristics;

public record AddCharacteristicsRequest(IEnumerable<NewCharacteristicsDto> Characteristics);

public record AddCharacteristicsResponse(IEnumerable<int> Ids);

public class AddCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/characteristics/", 
            async (ISender sender, AddCharacteristicsRequest request, CancellationToken token) =>
        {
            var result = await sender.Send(new AddCharacteristicsCommand(request.Characteristics), token);
            var response = new AddCharacteristicsResponse(result.Ids);
            return Results.Created("/articles/characteristics/", response);
        }).WithName("Создание характеристик артикула")
            .WithTags("Article Characteristics")
            .WithDescription("Создание характеристик артикула")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Создание характеристик артикула")
            .RequireAnyPermission("ARTICLE.CHARACTERISTICS.CREATE");
    }
}
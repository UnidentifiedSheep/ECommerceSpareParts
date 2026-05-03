using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ProductWeight.SetProductWeight;
using MediatR;

namespace Main.Api.EndPoints.ArticleWeight;

public record PutProductWeightRequest(decimal Weight, WeightUnit Unit);

public class PutProductWeightEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:int}/weights",
                async (ISender sender, int id, PutProductWeightRequest request, CancellationToken token) =>
                {
                    var command = new SetProductWeightCommand(id, request.Weight, request.Unit);
                    await sender.Send(command, token);
                    return Results.Created();
                }).WithTags("Article Weight")
            .WithDescription("Установка веса артикула.")
            .WithDisplayName("Установка веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_CREATE);
    }
}
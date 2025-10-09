using Carter;
using Core.Dtos.Amw.Sales;
using Main.Application.Handlers.Sales.GetSaleContent;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Sales;

public record GetSaleContentResponse(IEnumerable<SaleContentDto> Content);

public class GetSaleContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales/{id}/content", async (ISender sender, string id, CancellationToken cancellationToken) =>
            {
                var query = new GetSaleContentQuery(id);
                var result = await sender.Send(query, cancellationToken);
                var response = result.Adapt<GetSaleContentResponse>();
                return Results.Ok(response);
            }).RequireAuthorization("AMW")
            .WithTags("Sales")
            .WithDescription("Получение содержания продажи")
            .WithDisplayName("Получение содержания продажи");
    }
}
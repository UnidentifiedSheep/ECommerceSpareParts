using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Analytics;

namespace MonoliteUnicorn.EndPoints.Analytics.GetSaleChart;

public record GetSaleChartResponse(Dictionary<DateTime, List<ChartDto>> ChartsData);

public class GetSaleChartEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/analytics/sales/", async (ISender sender, DateTime startDate, 
            DateTime endDate, int? currencyId, CancellationToken token) =>
        {
            var query = new GetSaleChartQuery(startDate, endDate, currencyId);
            var result = await sender.Send(query, token);
            return Results.Ok(result.Adapt<GetSaleChartResponse>());
        }).RequireAuthorization("AMW")
        .WithGroup("Analytics")
        .WithDescription("Получить аналитику продаж за определенный период")
        .WithDisplayName("Получить аналитику продаж");
    }
}
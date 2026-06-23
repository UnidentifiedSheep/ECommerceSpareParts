using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales;
using MediatR;

namespace Main.Api.EndPoints.Internal;

public record InternalGetFullSaleResponse
{
    [JsonPropertyName("sale")]
    public required SaleDto Sale { get; init; }

    [JsonPropertyName("contents")]
    public required IEnumerable<SaleContentDto> Contents { get; init; }
}

public static class InternalSaleEndPoints
{
    public static RouteGroupBuilder AddInternalSaleEndPoints(this RouteGroupBuilder group)
    {
        var sale = group.MapGroup("/sales")
            .WithGroupName("Internal Sale")
            .WithTags("InternalSale");

        sale.MapGet("{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetFullSaleQuery(id), cancellationToken);
                return Results.Ok(new InternalGetFullSaleResponse
                {
                    Sale = result.Sale,
                    Contents = result.Contents
                });
            })
            .RequireAllPermissions(PermissionCodes.SALES_GET)
            .WithName("InternalFullSale")
            .WithDisplayName("Internal service full sale")
            .WithSummary("Получить полную продажу для внутреннего сервиса")
            .WithDescription("Получение продажи с ее содержимым для внутренних интеграций")
            .Produces<InternalGetFullSaleResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}

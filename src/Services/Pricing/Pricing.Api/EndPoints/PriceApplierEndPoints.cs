using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Carter;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Handlers.PriceApplier;
using Pricing.Application.Handlers.PriceApplier.GetPriceAppliers;
using Pricing.Application.Handlers.PriceApplier.UpsertPriceApplier;
using Pricing.Enums;

namespace Pricing.Api.EndPoints;

public record UpsertPriceApplierRequest
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("dslLogic")]
    public string? DslLogic { get; init; }

    [JsonPropertyName("states")]
    public required IReadOnlyList<UpsertPriceApplierStateDto> States { get; init; }
}

public record UpsertPriceApplierResponse
{
    [JsonPropertyName("applier")]
    public required PriceApplierDto Applier { get; init; }
}

public record GetPriceAppliersRequest
{
    [FromQuery(Name = "usage")]
    public PriceOfferSourceType Usage { get; init; }
}

public record GetPriceAppliersResponse
{
    [JsonPropertyName("appliers")]
    public required IReadOnlyList<PriceApplierDto> Appliers { get; init; }
}

public class PriceApplierEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var priceAppliers = app.MapGroup("/price-appliers")
            .WithTags("Price appliers");

        priceAppliers.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] GetPriceAppliersRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetPriceAppliersQuery(request.Usage);
                    var result = await sender.Send(query, cancellationToken);

                    return Results.Ok(
                        new GetPriceAppliersResponse
                        {
                            Appliers = result.Appliers
                        });
                })
            .WithName("GetPriceAppliers")
            .WithSummary("Получение правил формирования цены")
            .WithDescription("Возвращает локальные и динамические правила для выбранного источника предложения")
            .WithDisplayName("Получение правил формирования цены")
            .Produces<GetPriceAppliersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PRICE_APPLIERS_MANAGE);

        priceAppliers.MapPost(
                "",
                async (
                    ISender sender,
                    UpsertPriceApplierRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpsertPriceApplierCommand(
                        request.SystemName,
                        request.Name,
                        request.DslLogic,
                        request.States);
                    var result = await sender.Send(command, cancellationToken);

                    return Results.Ok(
                        new UpsertPriceApplierResponse
                        {
                            Applier = result.Applier
                        });
                })
            .WithName("UpsertPriceApplier")
            .WithSummary("Добавление или редактирование правила формирования цены")
            .WithDescription("Создаёт или обновляет локальное либо динамическое правило формирования цены")
            .WithDisplayName("Добавление или редактирование правила формирования цены")
            .Accepts<UpsertPriceApplierRequest>(false, "application/json")
            .Produces<UpsertPriceApplierResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PRICE_APPLIERS_MANAGE);
    }
}

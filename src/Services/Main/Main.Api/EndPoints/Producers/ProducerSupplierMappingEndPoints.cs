using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Application.Handlers.ProducerSupplierMappings;
using Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;
using Main.Application.Handlers.ProducerSupplierMappings.GetProducerSupplierMappings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Producers;

public record CreateProducerSupplierMappingRequest(
    Supplier Supplier,
    string SupplierProducerName);

public record CreateProducerSupplierMappingResponse(ProducerSupplierMappingDto ProducerSupplierMapping);

public record GetProducerSupplierMappingsResponse(IEnumerable<ProducerSupplierMappingDto> Mappings);

public record GetProducerSupplierMappingsRequest : PaginationQueryModel
{
    [FromQuery(Name = "supplier")]
    public Supplier[] Suppliers { get; init; } = [];
}

public static class ProducerSupplierMappingEndPoints
{
    public static RouteGroupBuilder MapProducerSupplierMappingEndPoints(this RouteGroupBuilder producers)
    {
        producers.MapPost(
                "/{producerId:int}/mappings/suppliers",
                async (
                    ISender sender,
                    int producerId,
                    CreateProducerSupplierMappingRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new CreateProducerSupplierMappingCommand(
                            new NewProducerSupplierMapping
                            {
                                ProducerId = producerId,
                                Supplier = request.Supplier,
                                SupplierProducerName = request.SupplierProducerName
                            }),
                        token);
                    return Results.Created(
                        $"/producers/{producerId}/mappings/suppliers/{result.ProducerSupplierMapping.Id}",
                        new CreateProducerSupplierMappingResponse(result.ProducerSupplierMapping));
                })
            .WithName("CreateProducerSupplierMapping")
            .WithSummary("Создать маппинг производителя поставщика")
            .WithDisplayName("Создание маппинга производителя поставщика")
            .WithDescription("Создание маппинга/референса производителя для поставщика")
            .Accepts<CreateProducerSupplierMappingRequest>(false, "application/json")
            .Produces<CreateProducerSupplierMappingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapDelete(
                "/{producerId:int}/mappings/suppliers/{mappingId:int}",
                async (
                    ISender sender,
                    int producerId,
                    int mappingId,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new DeleteProducerSupplierMappingCommand(mappingId), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteProducerSupplierMapping")
            .WithSummary("Удалить маппинг производителя поставщика")
            .WithDisplayName("Удаление маппинга производителя поставщика")
            .WithDescription("Удаление маппинга/референса производителя для поставщика")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapGet(
                "/{producerId:int}/mappings/suppliers",
                async (
                    ISender sender,
                    int producerId,
                    [AsParameters] GetProducerSupplierMappingsRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new GetProducerSupplierMappingsQuery(
                            producerId,
                            request.Suppliers,
                            request),
                        token);
                    return Results.Ok(new GetProducerSupplierMappingsResponse(result.Mappings));
                })
            .WithName("GetProducerSupplierMappings")
            .WithSummary("Получить маппинги производителя поставщика")
            .WithDisplayName("Получение маппингов производителя поставщика")
            .WithDescription("Получение маппингов/референсов производителя для поставщиков")
            .Produces<GetProducerSupplierMappingsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return producers;
    }
}

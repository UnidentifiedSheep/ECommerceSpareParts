using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers.AddOtherName;
using Main.Application.Handlers.Producers.CreateProducer;
using Main.Application.Handlers.Producers.DeleteOtherName;
using Main.Application.Handlers.Producers.DeleteProducer;
using Main.Application.Handlers.Producers.EditProducer;
using Main.Application.Handlers.Producers.GetProducerById;
using Main.Application.Handlers.Producers.GetProducerOtherNames;
using Main.Application.Handlers.Producers.GetProducers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints;

public record AddOtherNameToProducerRequest(string Alias);

public record CreateProducerRequest(NewProducerDto NewProducer);

public record CreateProducerResponse(ProducerDto Producer);

public record EditProducerRequest(PatchProducerDto EditProducer);

public record GetProducerOtherNamesResponse(IEnumerable<ProducerAliasDto> Names);

public record GetProducersResponse(IEnumerable<ProducerDto> Producers);

public record GetProducerByIdResponse(ProducerDto Producer);

public record PatchProducerResponse(ProducerDto Producer);

public record GetProducersRequest : PaginationQueryModel
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}

public class ProducersEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var producers = app.MapGroup("/producers")
            .WithTags("Producers");

        producers.MapPost(
                "/{producerId:int}/names",
                async (
                    ISender sender,
                    int producerId,
                    AddOtherNameToProducerRequest request,
                    CancellationToken token) =>
                {
                    await sender.Send(
                        new AddOtherNameCommand(
                            producerId,
                            request.Alias),
                        token);
                    return Results.Ok();
                })
            .WithName("AddProducerOtherName")
            .WithSummary("Добавить дополнительное имя производителя")
            .WithDisplayName("Добавление дополнительного имени")
            .WithDescription("Добавление дополнительного имени к производителю")
            .Accepts<AddOtherNameToProducerRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapDelete(
                "/{producerId:int}/names/{otherName}",
                async (
                    ISender sender,
                    int producerId,
                    string otherName,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new DeleteOtherNameCommand(producerId, otherName), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteProducerOtherName")
            .WithSummary("Удалить дополнительное имя производителя")
            .WithDisplayName("Удаление дополнительного имени")
            .WithDescription("Удаление дополнительного имени у производителя")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapGet(
                "/{producerId:int}/names",
                async (
                    ISender sender,
                    int producerId,
                    [AsParameters] PaginationQueryModel request,
                    CancellationToken token) =>
                {
                    var query = new GetProducerOtherNamesQuery(producerId, request);
                    var result = await sender.Send(query, token);
                    return Results.Ok(new GetProducerOtherNamesResponse(result.Names));
                })
            .WithName("GetProducerOtherNames")
            .WithSummary("Получить дополнительные имена производителя")
            .WithDisplayName("Получение дополнительных имен производителя")
            .WithDescription("Дополнительные имена производителя")
            .Produces<GetProducerOtherNamesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        producers.MapPost(
                "",
                async (
                    ISender sender,
                    CreateProducerRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new CreateProducerCommand(request.NewProducer), token);
                    return Results.Created("/producers", new CreateProducerResponse(result.Producer));
                })
            .WithName("CreateProducer")
            .WithSummary("Создать производителя")
            .WithDescription("Добавление новых производителей в бд")
            .WithDisplayName("Добавление производителей")
            .Accepts<CreateProducerRequest>(false, "application/json")
            .Produces<CreateProducerResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_CREATE);

        producers.MapPatch(
                "/{producerId:int}",
                async (
                    ISender sender,
                    int producerId,
                    EditProducerRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender
                        .Send(new EditProducerCommand(producerId, request.EditProducer), cancellationToken);
                    return Results.Ok(new PatchProducerResponse(result.Producer));
                })
            .WithName("EditProducer")
            .WithSummary("Редактировать производителя")
            .WithDescription("Редактирование производителя")
            .WithDisplayName("Редактирование производителя")
            .Accepts<EditProducerRequest>(false, "application/json")
            .Produces<PatchProducerResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapDelete(
                "/{id:int}",
                async (
                    ISender sender,
                    int id,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new DeleteProducerCommand(id), cancellationToken);
                    return Results.Ok();
                })
            .WithName("DeleteProducer")
            .WithSummary("Удалить производителя")
            .WithDescription("Удаление производителя из бд")
            .WithDisplayName("Удаление производителя")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_DELETE);

        producers.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] GetProducersRequest request,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetProducersQuery(request.SearchTerm, request), ct);
                    return Results.Ok(new GetProducersResponse(result.Producers));
                })
            .WithName("GetProducers")
            .WithSummary("Получить производителей")
            .WithDescription("Получение производителей по ключевому слову либо списком")
            .Produces<GetProducersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        producers.MapGet(
                "/{id:int}",
                async (
                    ISender sender,
                    int id,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetProducerByIdQuery(id), ct);
                    return Results.Ok(new GetProducerByIdResponse(result.Producer));
                })
            .WithName("GetProducerById")
            .WithSummary("Получить производителя по id")
            .WithDisplayName("Получение производителя по Id")
            .WithDescription("Получение производителя по Id")
            .Produces<GetProducerByIdResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
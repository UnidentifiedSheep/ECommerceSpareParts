using Abstractions.Models;
using Api.Common.Extensions;
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
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record AddOtherNameToProducerRequest(string OtherName, string WhereUsed);

public record CreateProducerRequest(NewProducerDto NewProducer);

public record CreateProducerResponse(int Id);

public record EditProducerRequest(PatchProducerDto EditProducer);

public record GetProducerOtherNamesResponse(IEnumerable<ProducerOtherNameDto> Names);

public record GetProducersResponse(IEnumerable<ProducerDto> Producers);

public record GetProducerByIdResponse(ProducerDto Producer);

public record PatchProducerResponse(ProducerDto Producer);

public class ProducersEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var producers = app.MapGroup("/producers")
            .WithTags("Producers");

        producers.MapPost("/{producerId:int}/names", async (
                ISender sender,
                int producerId,
                AddOtherNameToProducerRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new AddOtherNameCommand(producerId, request.OtherName, request.WhereUsed), token);
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

        producers.MapDelete("/{producerId:int}/names/{otherName}", async (
                ISender sender,
                int producerId,
                string otherName,
                string? usage,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteOtherNameCommand(producerId, otherName, usage!), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteProducerOtherName")
            .WithSummary("Удалить дополнительное имя производителя")
            .WithDisplayName("Удаление дополнительного имени")
            .WithDescription("Удаление дополнительного имени у производителя")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapGet("/{producerId:int}/names", async (
                ISender sender,
                int producerId,
                int page,
                int limit,
                CancellationToken token) =>
            {
                var query = new GetProducerOtherNamesQuery(producerId, new Pagination(page, limit));
                var result = await sender.Send(query, token);
                return Results.Ok(result.Adapt<GetProducerOtherNamesResponse>());
            })
            .WithName("GetProducerOtherNames")
            .WithSummary("Получить дополнительные имена производителя")
            .WithDisplayName("Получение дополнительных имен производителя")
            .WithDescription("Дополнительные имена производителя")
            .Produces<GetProducerOtherNamesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        producers.MapPost("", async (ISender sender, CreateProducerRequest request, CancellationToken token) =>
            {
                var result = await sender.Send(request.Adapt<CreateProducerCommand>(), token);
                return Results.Created("/producers", new CreateProducerResponse(result.ProducerId));
            })
            .WithName("CreateProducer")
            .WithSummary("Создать производителя")
            .WithDescription("Добавление новых производителей в бд")
            .WithDisplayName("Добавление производителей")
            .Accepts<CreateProducerRequest>(false, "application/json")
            .Produces<CreateProducerResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_CREATE);

        producers.MapPatch("/{producerId:int}", async (
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

        producers.MapDelete("/{id:int}", async (ISender sender, int id, CancellationToken cancellationToken) =>
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

        producers.MapGet("", async (ISender sender, string? searchTerm, int page, int limit) =>
            {
                var result = await sender.Send(new GetProducersQuery(searchTerm, new Pagination(page, limit)));
                return Results.Ok(result.Adapt<GetProducersResponse>());
            })
            .WithName("GetProducers")
            .WithSummary("Получить производителей")
            .WithDescription("Получение производителей по ключевому слову либо списком")
            .Produces<GetProducersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        producers.MapGet("/{id:int}", async (ISender sender, int id) =>
            {
                var result = await sender.Send(new GetProducerByIdQuery(id));
                return Results.Ok(result.Adapt<GetProducerByIdResponse>());
            })
            .WithName("GetProducerById")
            .WithSummary("Получить производителя по id")
            .WithDisplayName("Получение производителя по Id")
            .WithDescription("Получение производителя по Id")
            .Produces<GetProducerByIdResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

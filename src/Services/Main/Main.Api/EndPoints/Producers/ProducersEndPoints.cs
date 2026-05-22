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

public class ProducersEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var producers = app.MapGroup("/producers")
            .WithTags("Producers");

        producers.MapPost("/{producerId}/names", async (
                ISender sender,
                int producerId,
                AddOtherNameToProducerRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new AddOtherNameCommand(producerId, request.OtherName, request.WhereUsed), token);
                return Results.Ok();
            })
            .WithDisplayName("Добавление дополнительного имени")
            .WithDescription("Добавление дополнительного имени к производителю")
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapDelete("/{producerId}/names/{otherName}", async (
                ISender sender,
                int producerId,
                string otherName,
                string? usage,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteOtherNameCommand(producerId, otherName, usage!), cancellationToken);
                return Results.NoContent();
            })
            .WithDisplayName("Удаление дополнительного имени")
            .WithDescription("Удаление дополнительного имени у производителя")
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapGet("/{producerId}/names", async (
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
            .WithDisplayName("Получение дополнительных имен производителя")
            .WithDescription("Дополнительные имена производителя");

        producers.MapPost("", async (ISender sender, CreateProducerRequest request, CancellationToken token) =>
            {
                var result = await sender.Send(request.Adapt<CreateProducerCommand>(), token);
                return Results.Created("/producers", new CreateProducerResponse(result.ProducerId));
            })
            .WithDescription("Добавление новых производителей в бд")
            .WithDisplayName("Добавление производителей")
            .RequireAnyPermission(PermissionCodes.PRODUCERS_CREATE);

        producers.MapPatch("/{producerId}", async (
                ISender sender,
                int producerId,
                EditProducerRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new EditProducerCommand(producerId, request.EditProducer), cancellationToken);
                return Results.NoContent();
            })
            .WithDescription("Редактирование производителя")
            .WithDisplayName("Редактирование производителя")
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapDelete("/{id}", async (ISender sender, int id, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteProducerCommand(id), cancellationToken);
                return Results.Ok();
            })
            .WithDescription("Удаление производителя из бд")
            .WithDisplayName("Удаление производителя")
            .RequireAnyPermission(PermissionCodes.PRODUCERS_DELETE);

        producers.MapGet("", async (ISender sender, string? searchTerm, int page, int limit) =>
            {
                var result = await sender.Send(new GetProducersQuery(searchTerm, new Pagination(page, limit)));
                return Results.Ok(result.Adapt<GetProducersResponse>());
            })
            .WithDescription("Получение производителей по ключевому слову либо списком")
            .Produces<GetProducersResponse>();

        producers.MapGet("/{id}", async (ISender sender, int id) =>
            {
                var result = await sender.Send(new GetProducerByIdQuery(id));
                return Results.Ok(result.Adapt<GetProducerByIdResponse>());
            })
            .WithDisplayName("Получение производителя по Id")
            .WithDescription("Получение производителя по Id")
            .Produces<GetProducerByIdResponse>()
            .ProducesProblem(404);
    }
}

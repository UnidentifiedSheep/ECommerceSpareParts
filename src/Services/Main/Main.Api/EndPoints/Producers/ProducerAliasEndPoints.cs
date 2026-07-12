using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Producer.Aliases;
using Main.Application.Handlers.ProducerAliases;
using Main.Application.Handlers.ProducerAliases.AddAlias;
using Main.Application.Handlers.ProducerAliases.GetProducerAliases;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record AddAliasToProducerRequest(string Alias);

public record GetProducerAliasesResponse(IEnumerable<ProducerAliasDto> Aliases);

public static class ProducerAliasEndPoints
{
    public static RouteGroupBuilder MapProducerAliasEndPoints(this RouteGroupBuilder producers)
    {
        producers.MapPost(
                "/{producerId:int}/aliases",
                async (
                    ISender sender,
                    int producerId,
                    AddAliasToProducerRequest request,
                    CancellationToken token) =>
                {
                    await sender.Send(
                        new AddAliasCommand(
                            producerId,
                            request.Alias),
                        token);
                    return Results.Ok();
                })
            .WithName("AddProducerAlias")
            .WithSummary("Добавить дополнительное имя производителя")
            .WithDisplayName("Добавление дополнительного имени")
            .WithDescription("Добавление дополнительного имени к производителю")
            .Accepts<AddAliasToProducerRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapDelete(
                "/{producerId:int}/aliases/{alias}",
                async (
                    ISender sender,
                    int producerId,
                    string alias,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new DeleteAliasCommand(producerId, alias), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteProducerAlias")
            .WithSummary("Удалить дополнительное имя производителя")
            .WithDisplayName("Удаление дополнительного имени")
            .WithDescription("Удаление дополнительного имени у производителя")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PRODUCERS_EDIT);

        producers.MapGet(
                "/{producerId:int}/aliases",
                async (
                    ISender sender,
                    int producerId,
                    [AsParameters] PaginationQueryModel request,
                    CancellationToken token) =>
                {
                    var query = new GetProducerAliasesQuery(producerId, request);
                    var result = await sender.Send(query, token);
                    return Results.Ok(new GetProducerAliasesResponse(result.Aliases));
                })
            .WithName("GetProducerAliases")
            .WithSummary("Получить дополнительные имена производителя")
            .WithDisplayName("Получение дополнительных имен производителя")
            .WithDescription("Дополнительные имена производителя")
            .Produces<GetProducerAliasesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return producers;
    }
}

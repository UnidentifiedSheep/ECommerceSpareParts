using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using MediatR;
using Search.Application.Dtos.Producers;
using Search.Application.Handlers.Producers;

namespace Search.Api.EndPoints.Producers;

public record GetProducerAliasesResponse
{
    [JsonPropertyName("aliases")]
    public required IEnumerable<ProducerAlias> Aliases { get; init; }
}

public static class GetProducerAliasesEndPoint
{
    public static RouteGroupBuilder GetProducerAliases(this RouteGroupBuilder producers)
    {
        producers.MapGet(
                "/{producerId:int}/aliases",
                async (
                    int producerId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetProducerAliasesQuery(producerId),
                        cancellationToken);

                    return Results.Ok(
                        new GetProducerAliasesResponse
                        {
                            Aliases = result.Aliases
                        });
                })
            .WithTags("Producers")
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithDisplayName("Get producer other names")
            .Produces<GetProducerAliasesResponse>();

        return producers;
    }
}
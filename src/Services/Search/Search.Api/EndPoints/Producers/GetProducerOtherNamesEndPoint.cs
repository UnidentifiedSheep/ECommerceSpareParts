using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using MediatR;
using Search.Application.Dtos.Producers;
using Search.Application.Handlers.Producers.GetProducerOtherNames;

namespace Search.Api.EndPoints.Producers;

public record GetProducerOtherNamesResult
{
    [JsonPropertyName("otherNames")]
    public required IEnumerable<ProducerOtherNameDto> OtherNames { get; init; }
}

public static class GetProducerOtherNamesEndPoint
{
    public static RouteGroupBuilder GetProducerOtherNames(this RouteGroupBuilder producers)
    {
        producers.MapGet(
                "/{producerId:int}/other-names",
                async (
                    int producerId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetProducerOtherNamesQuery(producerId),
                        cancellationToken);

                    return Results.Ok(
                        new GetProducerOtherNamesResult
                        {
                            OtherNames = result.OtherNames
                        });
                })
            .WithTags("Producers")
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithDisplayName("Get producer other names")
            .Produces<GetProducerOtherNamesResult>();

        return producers;
    }
}
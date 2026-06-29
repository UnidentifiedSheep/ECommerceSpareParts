using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Application.Common.Dtos;
using Application.Common.Handlers.NamedObjects.GetNamedObjects;
using Carter;
using Enums;
using MediatR;

namespace Api.Common.EndPoints;

public record GetNamedObjectsResponse
{
    [JsonPropertyName("namedObjects")]
    public required IReadOnlyList<NamedObjectDto> NamedObjects { get; init; }
}

public class NamedObjectEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var namedObjects = app.MapGroup("/named-objects")
            .WithTags("Named Objects");

        namedObjects.MapGet("{groupName}", async (
                ISender sender,
                string groupName,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetNamedObjectsQuery(groupName), ct);

                return Results.Ok(new GetNamedObjectsResponse
                {
                    NamedObjects = result.NamedObjects
                });
            })
            .WithName("GetNamedObjects")
            .WithDisplayName("Get named objects")
            .Produces<GetNamedObjectsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAllPermissions(PermissionCodes.OPTIONS_GET);
    }
}

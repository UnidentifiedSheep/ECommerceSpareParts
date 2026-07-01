using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using MediatR;
using Pricing.Application.Dtos.Markup;
using Pricing.Application.Handlers.Markup.GetMarkups;
using Pricing.Application.Handlers.Markup.UpsertMarkupGroup;

namespace Pricing.Api.EndPoints;

public record UpsertMarkupGroupRequest
{
    [JsonPropertyName("group")]
    public required UpsertMarkupGroupDto Group { get; init; }
}

public record GetMarkupGroupsResponse
{
    [JsonPropertyName("groups")]
    public required IReadOnlyList<MarkupGroupDto> Groups { get; init; }
}

public class MarkupEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var markups = app.MapGroup("/markups")
            .WithTags("Markups");

        markups.MapPost(
                "",
                async (
                    ISender sender,
                    UpsertMarkupGroupRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpsertMarkupGroupCommand(request.Group);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithName("UpsertMarkupGroup")
            .WithSummary("Доюавление или редактирование группы наценок")
            .WithDescription("Доюавление или редактирование группы наценок")
            .WithDisplayName("Доюавление или редактирование группы наценок")
            .Accepts<UpsertMarkupGroupRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.MARKUP_CREATE);

        markups.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] PaginationQueryModel request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new GetMarkupsQuery(request);
                    var result = await sender.Send(command, cancellationToken);
                    return Results.Ok(
                        new GetMarkupGroupsResponse
                        {
                            Groups = result.Groups
                        });
                })
            .WithName("GetMarkupGroups")
            .WithSummary("Получение групп наценок")
            .WithDescription("Получение групп наценок")
            .WithDisplayName("Получение групп наценок")
            .Produces<GetMarkupGroupsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.MARKUP_GET);
    }
}
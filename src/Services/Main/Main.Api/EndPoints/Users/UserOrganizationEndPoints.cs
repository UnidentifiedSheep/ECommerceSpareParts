using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Organizations;
using Main.Application.Handlers.Organizations.CreateOrganization;
using Main.Application.Handlers.Organizations.GetOrganizations;
using Main.Enums.Organization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Users;

public record GetUserOrganizationsRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "ids")]
    public Guid[] Ids { get; init; } = [];

    [FromQuery(Name = "types")]
    public OrganizationType[] Types { get; init; } = [];
}

public record GetUserOrganizationsResponse(IReadOnlyList<OrganizationDto> Organizations);
public record CreateOrganizationRequest(string Name, string SystemName);
public record CreateOrganizationResponse(OrganizationDto Organization);

public static class UserOrganizationEndPoints
{
    public static RouteGroupBuilder MapUserOrganizationEndPoints(this RouteGroupBuilder users)
    {
        users.MapPost(
                "/{userId:guid}/organizations",
                async (
                    ISender sender,
                    Guid userId,
                    CreateOrganizationRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new CreateOrganizationCommand(
                            userId,
                            request.Name,
                            request.SystemName),
                        cancellationToken);

                    return Results.Created(
                        $"/organizations/{result.Organization.Id}",
                        new CreateOrganizationResponse(result.Organization));
                })
            .WithName("CreateOrganization")
            .WithSummary("Создать организацию")
            .WithDescription("Создание организации с указанным пользователем в роли владельца")
            .Accepts<CreateOrganizationRequest>("application/json")
            .Produces<CreateOrganizationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ORGANIZATIONS_CREATE);

        users.MapGet(
                "/{userId:guid}/organizations",
                async (
                    ISender sender,
                    Guid userId,
                    [AsParameters] GetUserOrganizationsRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetOrganizationsQuery(
                            request,
                            request.SortBy,
                            request.SearchTerm,
                            userId,
                            request.Ids,
                            request.Types),
                        cancellationToken);

                    return Results.Ok(
                        new GetUserOrganizationsResponse(result.Organizations));
                })
            .WithName("GetUserOrganizations")
            .WithSummary("Получить организации пользователя")
            .WithDescription("Получение организаций, участником которых является пользователь")
            .Produces<GetUserOrganizationsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ORGANIZATIONS_GET);

        return users;
    }
}

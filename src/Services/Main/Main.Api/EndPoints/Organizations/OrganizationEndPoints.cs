using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Organizations;
using Main.Application.Handlers.Organizations.GetOrganizations;
using Main.Enums.Organization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Organizations;

public record GetOrganizationsRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "ids")]
    public Guid[] Ids { get; init; } = [];

    [FromQuery(Name = "types")]
    public OrganizationType[] Types { get; init; } = [];
}

public record GetOrganizationsResponse(IReadOnlyList<OrganizationDto> Organizations);

public class OrganizationEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var organizations = app.MapGroup("/organizations")
            .WithTags("Organizations");

        organizations.MapOrganizationMemberEndPoints();

        organizations.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] GetOrganizationsRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetOrganizationsQuery(
                            request,
                            request.SortBy,
                            request.SearchTerm,
                            null,
                            request.Ids,
                            request.Types),
                        cancellationToken);

                    return Results.Ok(
                        new GetOrganizationsResponse(result.Organizations));
                })
            .WithName("GetOrganizations")
            .WithSummary("Найти организации")
            .WithDescription("Поиск организаций по названию и системному имени")
            .Produces<GetOrganizationsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ORGANIZATIONS_GET);

    }
}

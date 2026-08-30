using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Organizations;
using Main.Application.Handlers.Organizations;
using Main.Application.Handlers.Organizations.GetOrganizations;
using Main.Application.Handlers.Organizations.UpdateOrganization;
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

	[FromQuery(Name = "showHidden")]
	public bool? ShowHidden { get; init; }
}

public record GetOrganizationsResponse(IReadOnlyList<OrganizationListItemDto> Organizations);

public record GetOrganizationResponse(OrganizationDto Organization);

public record UpdateOrganizationRequest(PatchOrganizationDto Organization);

public record UpdateOrganizationResponse(OrganizationDto Organization);

public record IsOrganizationSystemNameAvailableResponse(bool IsAvailable);

public class OrganizationEndPoints : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		var organizations = app.MapGroup("/organizations").WithTags("Organizations");

		organizations.MapOrganizationMemberEndPoints();

		organizations
			.MapGet(
				"/{organizationId:guid}",
				async (
					ISender sender, Guid organizationId,
					CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(
						new GetOrganizationQuery(organizationId),
						cancellationToken);

					return Results.Ok(new GetOrganizationResponse(result.Organization));
				})
			.WithName("GetOrganizationById")
			.WithSummary("Получить организацию по идентификатору")
			.WithDescription("Получение организации по её идентификатору")
			.Produces<GetOrganizationResponse>()
			.ProducesProblem(StatusCodes.Status404NotFound)
			.RequireAnyPermission(PermissionCodes.ORGANIZATIONS_GET);

		organizations
			.MapGet(
				"/system-names/{systemName}",
				async (
					ISender sender, string systemName,
					CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(new GetOrganizationQuery(systemName), cancellationToken);

					return Results.Ok(new GetOrganizationResponse(result.Organization));
				})
			.WithName("GetOrganizationBySystemName")
			.WithSummary("Получить организацию по системному имени")
			.WithDescription("Получение организации по её системному имени")
			.Produces<GetOrganizationResponse>()
			.ProducesProblem(StatusCodes.Status404NotFound)
			.RequireAnyPermission(PermissionCodes.ORGANIZATIONS_GET);

		organizations
			.MapPatch(
				"/{organizationId:guid}",
				async (
					ISender sender, Guid organizationId,
					UpdateOrganizationRequest request, CancellationToken cancellationToken) =>
				{
					var organizationIdResult = await sender.Send(
						new UpdateOrganizationCommand(organizationId, request.Organization),
						cancellationToken);

					var result = await sender.Send(
						new GetOrganizationQuery(organizationIdResult.OrganizationId),
						cancellationToken);

					return Results.Ok(new UpdateOrganizationResponse(result.Organization));
				})
			.WithName("UpdateOrganization")
			.WithSummary("Обновить организацию")
			.WithDescription("Обновление изменяемых полей организации")
			.Accepts<UpdateOrganizationRequest>("application/json")
			.Produces<UpdateOrganizationResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.RequireAnyPermission(PermissionCodes.ORGANIZATIONS_EDIT);

		organizations
			.MapGet(
				"/system-names/{systemName}/availability",
				async (
					ISender sender, string systemName,
					CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(
						new IsOrganizationSystemNameAvailableQuery(systemName),
						cancellationToken);

					return Results.Ok(new IsOrganizationSystemNameAvailableResponse(result.IsAvailable));
				})
			.WithName("IsOrganizationSystemNameAvailable")
			.WithSummary("Проверить доступность системного имени организации")
			.WithDescription("Проверяет, не занято ли системное имя другой организацией")
			.WithDisplayName("Проверка доступности системного имени организации")
			.Produces<IsOrganizationSystemNameAvailableResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.RequireAnyPermission(PermissionCodes.ORGANIZATIONS_CREATE);

		organizations
			.MapGet(
				"",
				async (
					ISender sender, [AsParameters] GetOrganizationsRequest request,
					CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(
						new GetOrganizationsQuery(
							request,
							request.SortBy,
							request.SearchTerm,
							null,
							request.Ids,
							request.Types,
							request.ShowHidden ?? false),
						cancellationToken);

					return Results.Ok(new GetOrganizationsResponse(result.Organizations));
				})
			.WithName("GetOrganizations")
			.WithSummary("Найти организации")
			.WithDescription("Поиск организаций по названию и системному имени")
			.Produces<GetOrganizationsResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.RequireAnyPermission(PermissionCodes.ORGANIZATIONS_GET);

	}
}

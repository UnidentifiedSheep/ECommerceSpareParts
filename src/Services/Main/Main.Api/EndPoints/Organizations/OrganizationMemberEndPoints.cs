using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Organizations;
using Main.Application.Handlers.Organizations.AddOrganizationMember;
using Main.Application.Handlers.Organizations.ChangeOrganizationMemberRole;
using Main.Application.Handlers.Organizations.GetOrganizationMembers;
using Main.Application.Handlers.Organizations.RemoveOrganizationMember;
using Main.Enums.Organization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Organizations;

public record GetOrganizationMembersResponse(IReadOnlyList<OrganizationMemberDto> Members);
public record AddOrganizationMemberRequest(Guid UserId, OrganizationRole Role);
public record ChangeOrganizationMemberRoleRequest(OrganizationRole Role);

public static class OrganizationMemberEndPoints
{
    public static RouteGroupBuilder MapOrganizationMemberEndPoints(
        this RouteGroupBuilder organizations)
    {
        organizations.MapGet(
                "/{organizationId:guid}/members",
                async (
                    ISender sender,
                    Guid organizationId,
                    [AsParameters] PaginationQueryModel pagination,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetOrganizationMembersQuery(organizationId, pagination),
                        cancellationToken);

                    return Results.Ok(new GetOrganizationMembersResponse(result.Members));
                })
            .WithName("GetOrganizationMembers")
            .WithSummary("Получить участников организации")
            .WithDescription("Получение участников организации с пагинацией")
            .Produces<GetOrganizationMembersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ORGANIZATIONS_GET);

        organizations.MapPost(
                "/{organizationId:guid}/members",
                async (
                    ISender sender,
                    Guid organizationId,
                    AddOrganizationMemberRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new AddOrganizationMemberCommand(
                            organizationId,
                            request.UserId,
                            request.Role),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("AddOrganizationMember")
            .WithSummary("Добавить участника организации")
            .WithDescription("Добавление пользователя в организацию")
            .Accepts<AddOrganizationMemberRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ORGANIZATIONS_EDIT);

        organizations.MapPatch(
                "/{organizationId:guid}/members/{userId:guid}/role",
                async (
                    ISender sender,
                    Guid organizationId,
                    Guid userId,
                    ChangeOrganizationMemberRoleRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new ChangeOrganizationMemberRoleCommand(
                            organizationId,
                            userId,
                            request.Role),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("ChangeOrganizationMemberRole")
            .WithSummary("Изменить роль участника организации")
            .WithDescription("Изменение роли пользователя в организации")
            .Accepts<ChangeOrganizationMemberRoleRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ORGANIZATIONS_EDIT);

        organizations.MapDelete(
                "/{organizationId:guid}/members/{userId:guid}",
                async (
                    ISender sender,
                    Guid organizationId,
                    Guid userId,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new RemoveOrganizationMemberCommand(organizationId, userId),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("RemoveOrganizationMember")
            .WithSummary("Удалить участника организации")
            .WithDescription("Удаление пользователя из организации")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ORGANIZATIONS_EDIT);

        return organizations;
    }
}
